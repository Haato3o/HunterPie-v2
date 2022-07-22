from collections import OrderedDict
import requests
import xmltodict
import logging
from typing import Dict, List
import os

FORMAT = '%(asctime)s [%(levelname)s] %(message)s'
logging.basicConfig(format=FORMAT, level=logging.INFO)
logger = logging.getLogger("monster_data_converter")

remote_v1_monster_data = "https://raw.githubusercontent.com/Haato3o/HunterPie/master/HunterPie/HunterPie.Resources/Data/MonsterData.xml" 
v2_part_names = {
    "MONSTER_PART_UNKNOWN": "PART_UNKNOWN",
    "MONSTER_PART_REMOVABLE_TAIL": "PART_TAIL",
    "MONSTER_PART_HEAD": "PART_HEAD",
    "MONSTER_PART_BODY": "PART_BODY",
    "MONSTER_PART_LWING": "PART_L_WING",
    "MONSTER_PART_RWING": "PART_R_WING",
    "MONSTER_PART_LLEG": "PART_L_LEG",
    "MONSTER_PART_RLEG": "PART_R_LEG",
    "MONSTER_PART_TAIL": "PART_TAIL",
    "MONSTER_PART_BIG_FLINCH": "PART_BIG_FLINCH",
    "MONSTER_PART_REMOVABLE_HORNS": "PART_HORNS",
    "MONSTER_PART_HORN": "PART_HORN",
    "MONSTER_PART_ARMS": "PART_ARMS",
    "MONSTER_PART_LEGS": "PART_LEGS",
    "MONSTER_PART_KNOCKDOWN": "PART_KNOCKDOWN",
    "MONSTER_PART_NECK": "PART_NECK",
    "MONSTER_PART_CHEST": "PART_CHEST",
    "MONSTER_PART_LARM": "PART_L_ARM",
    "MONSTER_PART_RARM": "PART_R_ARM",
    "MONSTER_PART_REMOVABLE_HORNS_2": "PART_HORNS_2",
    "MONSTER_PART_RAGE": "PART_RAGE",
    "MONSTER_PART_LLIMBS": "PART_L_LIMBS",
    "MONSTER_PART_RLIMBS": "PART_R_LIMBS",
    "MONSTER_PART_WINGS": "PART_WINGS",
    "MONSTER_PART_LIMBS": "PART_LIMBS",
    "MONSTER_PART_WINGS": "PART_WINGS",
    "MONSTER_PART_ABDOMEN": "PART_ABDOMEN",
    "MONSTER_PART_REAR": "PART_REAR",
    "MONSTER_PART_COUNTERATTACK": "PART_COUNTERATTACK",
    "MONSTER_PART_REMOVABLE_HEAD": "PART_HEAD",
    "MONSTER_PART_HEAD_MUD": "PART_HEAD_MUD",
    "MONSTER_PART_BODY_MUD": "PART_BODY_MUD",
    "MONSTER_PART_ARMS_MUD": "PART_ARMS_MUD",
    "MONSTER_PART_LLEG_MUD": "PART_L_LEG_MUD",
    "MONSTER_PART_RLEG_MUD": "PART_R_LEG_MUD",
    "MONSTER_PART_TAIL_MUD": "PART_TAIL_MUD",
    "MONSTER_PART_JAW": "PART_JAW",
    "MONSTER_PART_EXPLOSION_WEAKENING": "PART_EXPLOSION_WEAKENING",
    "MONSTER_PART_CHARGE": "PART_CHARGE",
    "MONSTER_PART_BACK": "PART_BACK",
    "MONSTER_PART_TAIL_TIP": "PART_TAIL_TIP",
    "MONSTER_PART_THROAT": "PART_THROAT",
    "MONSTER_PART_FIN": "PART_FIN",
    "MONSTER_PART_INFLATED_TAIL": "PART_INFLATED_TAIL",
    "MONSTER_PART_HORNS": "PART_HORNS",
    "MONSTER_PART_SILVER_SPIKES_HEAD": "PART_SILVER_SPIKES_HEAD",
    "MONSTER_PART_SILVER_SPIKES_LARM": "PART_SILVER_SPIKES_L_ARM",
    "MONSTER_PART_SILVER_SPIKES_RARM": "PART_SILVER_SPIKES_R_ARM",
    "MONSTER_PART_SILVER_SPIKES_LWING": "PART_SILVER_SPIKES_L_WING",
    "MONSTER_PART_SILVER_SPIKES_RWING": "PART_SILVER_SPIKES_R_WING",
    "MONSTER_PART_REPEL": "PART_REPEL",
    "MONSTER_PART_SHELL": "PART_SHELL",
    "MONSTER_PART_EXHAUST_ORGAN_CENTRAL": "PART_EXHAUST_ORGAN_CENTRAL",
    "MONSTER_PART_EXHAUST_ORGAN_HEAD": "PART_EXHAUST_ORGAN_HEAD",
    "MONSTER_PART_EXHAUST_ORGAN_CRATER": "PART_EXHAUST_ORGAN_CRATER",
    "MONSTER_PART_EXHAUST_ORGAN_REAR": "PART_EXHAUST_ORGAN_REAR",
    "MONSTER_PART_WEAK_LSHELL": "PART_WEAK_L_SHELL",
    "MONSTER_PART_WEAK_RSHELL": "PART_WEAK_R_SHELL",
    "MONSTER_PART_ROCK": "PART_ROCK",
    "MONSTER_PART_MANE": "PART_MANE",
    "MONSTER_PART_REMOVABLE_BALLOON": "PART_BALLOON",
    "MONSTER_PART_BALLOON": "PART_BALLOON",
    "MONSTER_PART_SKY_FALL": "PART_SKY_FALL",
    "MONSTER_PART_LBONE": "PART_L_BONE",
    "MONSTER_PART_RBONE": "PART_R_BONE",
    "MONSTER_PART_EMISSIONS": "PART_EMISSIONS",
    "MONSTER_PART_HORNS_GOLD": "PART_HORNS_GOLD",
    "MONSTER_PART_MANE_GOLD": "PART_MANE_GOLD",
    "MONSTER_PART_LCHEST_GOLD": "PART_L_CHEST_GOLD",
    "MONSTER_PART_RCHEST_GOLD": "PART_R_CHEST_GOLD",
    "MONSTER_PART_LARM_GOLD": "PART_L_ARM_GOLD",
    "MONSTER_PART_RARM_GOLD": "PART_R_ARM_GOLD",
    "MONSTER_PART_LLEG_GOLD": "PART_L_LEG_GOLD",
    "MONSTER_PART_RLEG_GOLD": "PART_R_LEG_GOLD",
    "MONSTER_PART_LTAIL_GOLD": "PART_L_TAIL_GOLD",
    "MONSTER_PART_RTAIL_GOLD": "PART_R_TAIL_GOLD",
    "MONSTER_PART_GLOWING_HEAD": "PART_GLOWING_HEAD",
    "MONSTER_PART_GLOWING_TAIL": "PART_GLOWING_TAIL",
    "MONSTER_PART_EMERGE_SNOW_HEAD": "PART_EMERGE_SNOW_HEAD",
    "MONSTER_PART_EMERGE_SNOW_BODY": "PART_EMERGE_SNOW_BODY",
    "MONSTER_PART_EMERGE_SNOW_TAIL": "PART_EMERGE_SNOW_TAIL",
    "MONSTER_PART_HEAD_SNOW": "PART_HEAD_SNOW",
    "MONSTER_PART_BODY_SNOW": "PART_BODY_SNOW",
    "MONSTER_PART_TAIL_SNOW": "PART_TAIL_SNOW",
    "MONSTER_PART_SKY_FALL": "PART_SKY_FALL",
    "MONSTER_PART_HEAD_ICE": "PART_HEAD_ICE",
    "MONSTER_PART_BODY_ICE": "PART_BODY_ICE",
    "MONSTER_PART_WINGS_ICE": "PART_WINGS_ICE",
    "MONSTER_PART_ARMS_ICE": "PART_ARMS_ICE",
    "MONSTER_PART_REMOVABLE_HEAD_ROCK": "PART_HEAD_ROCK",
    "MONSTER_PART_REMOVABLE_CHEST_ROCK": "PART_CHEST_ROCK",
    "MONSTER_PART_LNECK_ROCK": "PART_L_NECK_ROCK",
    "MONSTER_PART_RNECK_ROCK": "PART_R_NECK_ROCK",
    "MONSTER_PART_HEAD_ROCK": "PART_HEAD_ROCK",
    "MONSTER_PART_TAIL_ROCK": "PART_TAIL_ROCK",
    "MONSTER_PART_LWING_ROCK": "PART_L_WING_ROCK",
    "MONSTER_PART_RWING_ROCK": "PART_R_WING_ROCK",
    "MONSTER_PART_LARM_ROCK": "PART_L_ARM_ROCK",
    "MONSTER_PART_RARM_ROCK": "PART_R_ARM_ROCK",
    "MONSTER_PART_BODY_LEGS": "PART_BODY_LEGS",
    "MONSTER_PART_ANTLERS": "PART_ANTLERS"
}

def load_v1_data() -> Dict:
    logger.info("Requesting remote v1 data")
    req = requests.get(remote_v1_monster_data)

    logger.info("Parsing XML into Dictionary")
    return xmltodict.parse(req.text)

def save_v2_data(data: OrderedDict):
    if not os.path.exists("output"):
        os.mkdir("output")

    with open("output/out_monster_data_v2.xml", "w") as output:
        xmltodict.unparse(data, output, 'UTF-8', pretty = True, short_empty_elements = True)

def extract_crown(monster) -> OrderedDict:
    crowns = OrderedDict()

    crowns["@Mini"] = monster["Crown"]["@Mini"]
    crowns["@Silver"] = monster["Crown"]["@Silver"]
    crowns["@Gold"] = monster["Crown"]["@Gold"]
    
    return crowns

def extract_weaknesses(monster) -> OrderedDict:
    weaknesses = OrderedDict(Weakness = [])

    if monster["Weaknesses"] is None:
        return OrderedDict()
        
    for weakness in monster["Weaknesses"]["Weakness"]:
        v2_weakness = OrderedDict()
        weakness_id: str =  weakness["@ID"]
        v2_weakness["@Name"] = weakness_id.removeprefix("ELEMENT_").lower().capitalize()
        weaknesses["Weakness"].append(v2_weakness)

    return weaknesses


def get_equivalent_v2_name(partId: str) -> str:
    if partId not in v2_part_names:
        logger.info("Missing name for %s", partId)
        return partId

    return v2_part_names[partId]

def extract_parts(monster) -> List[OrderedDict]:
    parts = OrderedDict(Part = [])

    id = 0
    for part in monster["Parts"]["Part"]:
        data = OrderedDict()

        data["@Id"] = id if part.get("@Index") == None else part["@Index"]
        data["@String"] = get_equivalent_v2_name(part["@Name"])

        if part.get("@IsRemovable") is not None:
            data["@IsSeverable"] = part.get("@IsRemovable")
        
        if part.get("@TenderizeIds") is not None:
            data["@TenderizeIds"] = part["@TenderizeIds"]

        parts["Part"].append(data)
        id += 1

    return parts

def iterate_monsters(monsters: OrderedDict) -> List[OrderedDict]:
    monster_list = []

    for monster in monsters:
        node = OrderedDict()
        node["@Id"] = monster["@GameID"]
        node["Crowns"] = extract_crown(monster)
        node["Weaknesses"] = extract_weaknesses(monster)
        node["Parts"] = extract_parts(monster)
        monster_list.append(node)

    logger.info(f"Converted {len(monsters)} monsters")

    monster_list = sorted(monster_list, key = lambda x: int(x['@Id']))

    return monster_list

def iterate_ailments(ailments: OrderedDict) -> OrderedDict:
    ailments_list = []

    for ailment in ailments:
        node = OrderedDict()
        node["@Id"] = ailment["@Id"]
        node["@String"] = ailment["@Name"].replace("STATUS_", "AILMENT_")
        ailments_list.append(node)

    return ailments_list


def generate_v2_data(ailments: List[OrderedDict], monsters: List[OrderedDict]) -> OrderedDict:
    ailmentsNode = OrderedDict(Ailment = ailments)
    monstersNode = OrderedDict(Monster = monsters)

    root = OrderedDict(GameData = OrderedDict(
        Ailments = ailmentsNode,
        Monsters = monstersNode
    ))
    return root

def main():
    v1_data = load_v1_data()

    v2_monsters = iterate_monsters(v1_data["Monsters"]["Monster"])
    v2_ailments = iterate_ailments(v1_data["Monsters"]["Ailments"]["Ailment"])
    v2_data = generate_v2_data(v2_ailments, v2_monsters)

    save_v2_data(v2_data)


if __name__ == "__main__":
    
    logger.info("Starting v1 monster data conversion")
    main()
    logger.info("Finished converting v1 monster data to v2")