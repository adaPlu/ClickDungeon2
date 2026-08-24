#!/usr/bin/env python3
import json, sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
CONTENT=ROOT/'Assets'/'ClickDungeon'/'Content'/'Json'
required=['classes.json','abilities.json','monsters.json','bosses.json','biomes.json','floor_archetypes.json','items.json','affixes.json','statuses.json','balance.json','loot_tables.json','shops.json','monster_variants.json','traps.json','progression.json','achievements.json','content_migrations.json']
errors=[];data={}
for name in required:
    p=CONTENT/name
    if not p.exists(): errors.append(f'missing required file {name}'); continue
    try: data[name]=json.loads(p.read_text(encoding='utf-8'))
    except Exception as e: errors.append(f'{name}: invalid JSON: {e}')
for p in CONTENT.glob('*.json'):
    if p.name in data: continue
    try: data[p.name]=json.loads(p.read_text(encoding='utf-8'))
    except Exception as e: errors.append(f'{p.name}: invalid JSON: {e}')

def ids(file,key):
    vals=[]
    for row in data.get(file,{}).get(key,[]):
        i=row.get('id')
        if not i: errors.append(f'{file}: entry missing id')
        else: vals.append(i)
    if len(vals)!=len(set(vals)): errors.append(f'{file}: duplicate ids')
    return set(vals)

classes=ids('classes.json','classes');abilities=ids('abilities.json','abilities');monsters=ids('monsters.json','monsters');bosses=ids('bosses.json','bosses');biomes=ids('biomes.json','biomes');items=ids('items.json','items');affixes=ids('affixes.json','affixes');statuses=ids('statuses.json','statuses');archetypes=ids('floor_archetypes.json','archetypes');traps=ids('traps.json','traps');achievements=ids('achievements.json','achievements');variants=ids('monster_variants.json','variants')

for a in data.get('abilities.json',{}).get('abilities',[]):
    if a.get('class_id') not in classes: errors.append(f"ability {a.get('id')} references missing class {a.get('class_id')}")
    if a.get('max_charges',0)<=0 or a.get('recharge_progress_required',0)<=0: errors.append(f"ability {a.get('id')} has invalid charge/recharge values")
for c in classes:
    rows=sorted((a for a in data.get('abilities.json',{}).get('abilities',[]) if a.get('class_id')==c),key=lambda x:(x.get('unlock_mastery',0),x.get('id','')))
    if len(rows) not in (1,5): errors.append(f'{c}: expected 5 abilities for production content (or 1 in early slice), got {len(rows)}')
    thresholds=[a.get('unlock_mastery',0) for a in rows]
    if thresholds!=sorted(thresholds): errors.append(f'{c}: mastery thresholds are not monotonic')
allowed_classes={'class.knight','class.ranger','class.thief','class.wizard'}
allowed_threats={'none','adjacent','cross_two','orthogonal_line','aura_two'}
allowed_intents={'attack','heavy_attack','steal_gold','poison','guard','summon','hazard'}
if classes!=allowed_classes: errors.append(f'classes.json class ids must be exactly {sorted(allowed_classes)}, found {sorted(classes)}')
for row in list(data.get('monsters.json',{}).get('monsters',[]))+list(data.get('bosses.json',{}).get('bosses',[])):
    if row.get('threat') not in allowed_threats: errors.append(f"{row.get('id')} has unsupported threat {row.get('threat')}")
    if row.get('intent') not in allowed_intents: errors.append(f"{row.get('id')} has unsupported intent {row.get('intent')}")
    if row.get('intent_power',0)<=0: errors.append(f"{row.get('id')} has invalid intent_power")
    if not row.get('decision'): errors.append(f"{row.get('id')} missing decision rationale")

for m in data.get('monsters.json',{}).get('monsters',[]):
    for b in m.get('biomes',[]):
        if b not in biomes: errors.append(f"monster {m.get('id')} references missing biome {b}")
for b in data.get('bosses.json',{}).get('bosses',[]):
    f=b.get('floor',0)
    if not 1<=f<=data.get('balance.json',{}).get('campaign_floors',50): errors.append(f"boss {b.get('id')} invalid floor {f}")
for trap in data.get('traps.json',{}).get('traps',[]):
    status=trap.get('status')
    if status and status not in statuses: errors.append(f"trap {trap.get('id')} references missing status {status}")
    if status and trap.get('status_duration',0)<=0: errors.append(f"trap {trap.get('id')} needs positive status_duration")
for status in data.get('statuses.json',{}).get('statuses',[]):
    if not status.get('effect'): errors.append(f"status {status.get('id')} missing effect")
    if status.get('default_duration',0)<=0: errors.append(f"status {status.get('id')} invalid default_duration")
    if status.get('max_stacks',0)<=0: errors.append(f"status {status.get('id')} invalid max_stacks")

loot=data.get('loot_tables.json',{}).get('tables',[])
for table in loot:
    if not table.get('entries'): errors.append(f"loot {table.get('id')} has no entries")
    for e in table.get('entries',[]):
        i=e.get('id','')
        if not (i in items or i.startswith('currency.')): errors.append(f"loot {table.get('id')} references missing {i}")
        if e.get('weight',0)<=0: errors.append(f"loot {table.get('id')} has non-positive weight for {i}")
for shop in data.get('shops.json',{}).get('shops',[]):
    for i in shop.get('stock',[]):
        if i not in items: errors.append(f"shop {shop.get('id')} references missing {i}")

mastery=data.get('progression.json',{}).get('class_mastery',{})
for field in ('monster_defeat_reward','boss_mastery_rewards','forbidden_floor_mastery_bonus','campaign_completion_bonus','abyss_depth_reward','abyss_milestone_interval','abyss_milestone_bonus'):
    if field not in mastery: errors.append(f'progression.json missing class_mastery.{field}')
boss_rewards=mastery.get('boss_mastery_rewards',[])
if len(boss_rewards)!=len(data.get('bosses.json',{}).get('bosses',[])): errors.append('progression boss_mastery_rewards must match boss count')
if data.get('progression.json',{}).get('campaign',{}).get('prestige_depth',0)<=data.get('balance.json',{}).get('campaign_floors',50): errors.append('prestige depth must exceed campaign length')

allowed_triggers={'floor.entered','floor.entered.forbidden','vault.opened','campaign.completed','abyss.depth.entered'}
for a in data.get('achievements.json',{}).get('achievements',[]):
    if not a.get('display_name'): errors.append(f"achievement {a.get('id')} missing display_name")
    if a.get('trigger') not in allowed_triggers: errors.append(f"achievement {a.get('id')} has unsupported trigger {a.get('trigger')}")

all_current=classes|abilities|monsters|bosses|biomes|items|affixes|statuses|archetypes|traps|achievements|variants
migrations=data.get('content_migrations.json',{}).get('migrations',{})
for old,new in migrations.items():
    if old==new: errors.append(f'content migration {old} maps to itself')
    if new not in all_current: errors.append(f'content migration {old} targets missing id {new}')

if errors:
    print('CONTENT VALIDATION FAILED')
    for e in errors: print(' -',e)
    sys.exit(1)
print(f'CONTENT VALIDATION PASSED: {len(list(CONTENT.glob("*.json")))} JSON files')
