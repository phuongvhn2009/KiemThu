"""Run from any directory: python Checks/check_lab4.py (standard library only)."""
from pathlib import Path
import re
import sys
import hashlib
import json
import zipfile
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
pages = root / 'SauceDemo.Tests/Pages'
tests = root / 'SauceDemo.Tests/Tests'
for path in tests.glob('*.cs'):
    assert 'By' not in path.read_text(encoding='utf-8-sig'), f'Locator leaked into {path}'
for path in pages.glob('*.cs'):
    source = path.read_text(encoding='utf-8-sig')
    assert 'Assert' not in source and 'NUnit' not in source, f'Assertion in {path}'
    assert not re.search(r'public\s+(?:IWebElement|IReadOnlyList<IWebElement>|IEnumerable<IWebElement>)\b', source), f'Public element API: {path}'
    assert not re.search(r'Thread\.Sleep|Task\.Delay|ImplicitWait', source), f'Non-explicit wait: {path}'
    if path.name != 'BasePage.cs':
        assert ': BasePage(' in source, f'Page does not inherit BasePage: {path}'
        assert 'new WebDriverWait' not in source, f'Page creates its own wait: {path}'
        assert not re.search(r'\b(?:IWebDriver|WebDriverWait)\s+\w+\s*[;={]', source), f'Page redeclares driver/wait: {path}'
        for line in source.splitlines():
            if 'By.' in line:
                assert 'private readonly By ' in line, f'Inline/non-private locator: {path}: {line}'
base = (pages / 'BasePage.cs').read_text(encoding='utf-8-sig')
assert 'abstract class BasePage' in base and 'WebDriverWait Wait' in base and 'Tim(By locator)' in base
ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
evidence = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else root / 'docs/evidence/lab4/session-20261114'
def results(name):
    tree = ET.parse(evidence / name)
    return {r.attrib['testName']: r.attrib['outcome'] for r in tree.findall('.//t:UnitTestResult', ns)}
before, after = results('before.trx'), results('after.trx')
assert len(before) == 18 and sum(v == 'Passed' for v in before.values()) == 17, before
assert before.get('DynamicLoadingWithoutWait') == 'NotExecuted', before
assert before == after, f'Test identities/outcomes changed: {before} vs {after}'
intentional = ET.parse(evidence / 'intentional-after.trx')
assert results('intentional-after.trx') == {'DynamicLoadingWithoutWait': 'Failed'}
assert 'NoSuchElementException' in ''.join(intentional.getroot().itertext())
manifest = evidence / 'before-source-sha256.json'
if manifest.exists():
    for name, digest in json.loads(manifest.read_text(encoding='utf-8')).items():
        if '/Framework/' in name or '/Data/' in name or name.endswith(('.csproj', 'appsettings.json')):
            assert hashlib.sha256((root / name).read_bytes()).hexdigest() == digest, f'Baseline configuration/data/framework changed: {name}'
    with zipfile.ZipFile(evidence / 'before-source.zip') as snapshot:
        for name in snapshot.namelist():
            if '/Tests/' in name and name.endswith('.cs'):
                before_asserts = [line.strip() for line in snapshot.read(name).decode('utf-8-sig').splitlines() if 'Assert.' in line]
                after_asserts = [line.strip() for line in (root / name).read_text(encoding='utf-8-sig').splitlines() if 'Assert.' in line]
                assert before_asserts == after_asserts, f'Baseline assertion changed: {name}'
print('PASS: Pages/Tests conventions; same 17 test names and PASS outcomes; intentional failure preserved.')
