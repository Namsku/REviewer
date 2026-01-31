import json
import os

RE_DATA_PATH = r"e:\Projects\SRTs\REviewer\REviewer\Resources\Files\re-data.json"
OUTPUT_PATH = "game_data.json"

def extract():
    if not os.path.exists(RE_DATA_PATH):
        print(f"Error: {RE_DATA_PATH} not found")
        return

    with open(RE_DATA_PATH, 'r', encoding='utf-8') as f:
        data = json.load(f)

    result = {}

    for game_key, game_config in data.items():
        key_items = []
        item_ids = game_config.get("ItemIDs", {})
        dup_items = game_config.get("DupItems", {})

        for item_id, item_props in item_ids.items():
            item_type = item_props.get("Type", "")
            if item_id == "255" or item_props.get("Name") == "Nothing":
                continue

            if item_type in ["Key Item", "Optionnal Key Item"]:
                name = item_props.get("Name")
                img = item_props.get("Img")
                
                # Normalize image path for web (assuming same structure or serving)
                # Client sends: "Resources/RE1/SwordKey.png"
                # We want just the filename or a relative path the server can serve
                
                count = dup_items.get(name, 1)
                for i in range(count):
                    key_items.append({
                        "name": name,
                        "img": img,
                        "type": item_type
                    })
        
        result[game_key] = key_items

    with open(OUTPUT_PATH, 'w', encoding='utf-8') as f:
        json.dump(result, f, indent=2)
    
    print(f"Successfully extracted key items to {OUTPUT_PATH}")

if __name__ == "__main__":
    extract()
