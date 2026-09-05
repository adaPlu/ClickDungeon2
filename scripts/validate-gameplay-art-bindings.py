#!/usr/bin/env python3
from pathlib import Path
import json
import re
import sys

ROOT=Path(sys.argv[1]).resolve() if len(sys.argv)>1 else Path(__file__).resolve().parents[1]
SOURCE=ROOT/'Assets'/'ClickDungeon'/'Art'/'Source'
RUNTIME=ROOT/'Assets'/'ClickDungeon'/'Art'/'Runtime'
CONTENT=ROOT/'Assets'/'ClickDungeon'/'Content'/'Json'
BINDINGS=SOURCE/'gameplay_art_bindings.json'
EXPECTED_SCHEMA='clickdungeon.gameplay_art_bindings.v1'
errors=[]


def load_json(path):
    try:
        return json.loads(path.read_text(encoding='utf-8'))
    except FileNotFoundError:
        errors.append(f'missing required file {path.relative_to(ROOT)}')
    except Exception as exc:
        errors.append(f'invalid JSON {path.relative_to(ROOT)}: {exc}')
    return {}


def content_ids(filename,key):
    data=load_json(CONTENT/filename)
    return {row.get('id') for row in data.get(key,[]) if row.get('id')}


def require_unique(rows,field,label):
    seen=set()
    for row in rows:
        value=row.get(field)
        if not value:
            continue
        if value in seen:
            errors.append(f'duplicate {label} {value}')
        seen.add(value)


def register_presentation_key(key,owner,seen):
    if not key:
        return
    previous=seen.get(key)
    if previous and previous!=owner:
        errors.append(f'duplicate presentation key {key}: {previous} and {owner}')
    else:
        seen[key]=owner


def hero_ids_from_catalog():
    path=ROOT/'Assets'/'ClickDungeon'/'Application'/'Heroes'/'HeroIdentityCatalog.cs'
    if not path.exists():
        return None
    text=path.read_text(encoding='utf-8')
    return set(re.findall(r'new HeroIdentityDefinition\("([^"]+)"',text))


data=load_json(BINDINGS)
if data.get('schema')!=EXPECTED_SCHEMA:
    errors.append(f"unexpected gameplay art binding schema {data.get('schema')!r}; expected {EXPECTED_SCHEMA}")
if data.get('game')!='ClickDungeon':
    errors.append('gameplay art bindings must use player-facing game name ClickDungeon')
policy=data.get('policy',{})
if policy and policy.get('player_facing_brand')!='ClickDungeon':
    errors.append('policy.player_facing_brand must be ClickDungeon')

items=data.get('items',[])
monsters=data.get('monsters',[])
heroes=data.get('heroes',[])
environment=data.get('environment',[])
for section_name,rows in [('items',items),('monsters',monsters),('heroes',heroes),('environment',environment)]:
    if not isinstance(rows,list):
        errors.append(f'{section_name} must be an array')
        continue
    for row in rows:
        if not isinstance(row,dict):
            errors.append(f'{section_name} contains a non-object row')
        elif not row.get('status'):
            errors.append(f'{section_name} binding is missing status: {row}')

item_ids=content_ids('items.json','items')
monster_ids=content_ids('monsters.json','monsters')
biome_ids=content_ids('biomes.json','biomes')
hero_ids=hero_ids_from_catalog()

require_unique(items,'gameplay_id','item gameplay_id')
require_unique(monsters,'gameplay_id','monster gameplay_id')
require_unique(heroes,'hero_id','hero_id')

for row in items:
    gameplay_id=row.get('gameplay_id')
    if gameplay_id and gameplay_id not in item_ids:
        errors.append(f'unknown item gameplay_id {gameplay_id}')
for row in monsters:
    gameplay_id=row.get('gameplay_id')
    if gameplay_id and gameplay_id not in monster_ids:
        errors.append(f'unknown monster gameplay_id {gameplay_id}')
for row in environment:
    gameplay_id=row.get('gameplay_id')
    if gameplay_id and gameplay_id not in biome_ids:
        errors.append(f'unknown environment gameplay_id {gameplay_id}')
for row in heroes:
    hero_id=row.get('hero_id')
    if hero_id and hero_ids is not None and hero_id not in hero_ids:
        errors.append(f'unknown hero_id {hero_id}')

presentation_keys={}
for row in items:
    register_presentation_key(row.get('presentation_key'),row.get('gameplay_id','item'),presentation_keys)
for row in monsters:
    register_presentation_key(row.get('current_presentation_key'),row.get('gameplay_id','monster'),presentation_keys)
for row in environment:
    register_presentation_key(row.get('presentation_key') or row.get('current_presentation_key'),row.get('gameplay_id','environment'),presentation_keys)
for row in heroes:
    owner=row.get('hero_id','hero')
    keys=row.get('canonical_keys',[])
    if not isinstance(keys,list):
        errors.append(f'{owner}: canonical_keys must be an array')
        continue
    if len(keys)!=len(set(keys)):
        errors.append(f'{owner}: duplicate canonical_keys')
    for key in keys:
        register_presentation_key(key,owner,presentation_keys)

# Bindings that claim an existing canonical runtime file must point at a real file.
existing_statuses={'existing_canonical_reuse','legacy_runtime_present_approved_replacement_pending','backdrop_present_modular_room_kit_pending'}
for section in (items,monsters,environment):
    for row in section:
        runtime_file=row.get('runtime_file')
        if runtime_file and row.get('status') in existing_statuses and not (RUNTIME/runtime_file).exists():
            errors.append(f"{row.get('gameplay_id') or row.get('presentation_key')}: declared runtime file missing: {runtime_file}")

if errors:
    print('GAMEPLAY ART BINDING VALIDATION FAILED')
    for error in errors:
        print(' -',error)
    sys.exit(1)

print(f'GAMEPLAY ART BINDING VALIDATION PASSED: {len(heroes)} heroes, {len(items)} items, {len(monsters)} monsters, {len(environment)} environment bindings')
