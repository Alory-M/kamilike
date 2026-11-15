"""Validate the imported catalog and replay original solutions, without Unity."""
import json
from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1] / 'Assets/Resources'
levels = json.loads((ROOT / 'levels.json').read_text())['levels']
assert len(levels) == 72
assert len({level['id'] for level in levels}) == len(levels)
for level in levels:
    w, h = level['width'], level['height']
    cells = list(level['colours'])
    assert len(cells) == w * h
    xml, = list((ROOT / 'puzzles').rglob(level['id'] + '.xml'))
    original = ET.parse(xml).getroot()
    assert original.get('colours') == level['colours']
    assert int(original.get('gold')) == level['gold']
    theme = level['theme'].replace('_three', '')
    atlas = ET.parse(ROOT / 'themeTextures_png' / (theme + '_full.xml')).getroot()
    sprites = {entry.get('name') for entry in atlas}
    for c in set(cells):
        assert f'sq{c}_static_0000' in sprites, (level['id'], c)
    assert (ROOT / 'themeTextures_png' / (theme + '_full.png')).is_file()
    turns = original.findall('./Solution/Turn')
    assert turns
    for turn in turns:
        i = int(turn.get('num')) if turn.get('num') else int(turn.get('y')) * w + int(turn.get('x'))
        assert 0 <= i < len(cells)
        old, new = cells[i], turn.get('colour')
        queue, seen = [i], {i}
        while queue:
            i = queue.pop()
            cells[i] = new
            x, y = i % w, i // w
            neighbours = ([i-1] if x else []) + ([i+1] if x<w-1 else []) + ([i-w] if y else []) + ([i+w] if y<h-1 else [])
            for j in neighbours:
                if j not in seen and cells[j] == old:
                    seen.add(j)
                    queue.append(j)
    assert len(set(cells)) == 1, level['id']
print(f'PASS: {len(levels)} levels; dimensions, IDs, textures and original solutions verified.')
