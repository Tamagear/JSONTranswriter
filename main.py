# This is a script to covert a JSON file of card data to a chroma face.
import datetime
import json as js
import os.path
import pathlib
import sys
import time
import tkinter.filedialog
from PIL import Image
from PIL import ImageDraw
from PIL import ImageFont
from bing_image_downloader import downloader
from random import randrange
import shutil

use_local_database = True
output_directory = os.path.join(os.path.expanduser('~/Desktop'), 'CHROMA Deck Atlas Files\\')

path_base_card = 'content/card_base.png'
path_unit_additionals = 'content/unit_additionals.png'
name_font = ImageFont.truetype('content/ImalsrithV2-Regular.ttf', 140)
details_font = ImageFont.truetype('content/ImalsrithV2-Regular.ttf', 110)
text_font = ImageFont.truetype('content/SecularOne-Regular.ttf', 65)
stats_font = ImageFont.truetype('content/SecularOne-Regular.ttf', 110)
temp_image_dir = os.path.join(pathlib.Path().resolve(), 'content\\tmp\\card_images\\')
card_image_dir = os.path.join(pathlib.Path().resolve(), 'content\\tmp\\card_images\\finals\\')
local_database = 'content/database/database.json'
online_database_link = ''
card_sprite_size = 1335, 760
inner_card_sprite_offset = 200  # TODO: Online Database


def json_to_face():
    if os.path.exists(temp_image_dir):
        shutil.rmtree(temp_image_dir)

    os.mkdir(temp_image_dir)
    os.mkdir(card_image_dir)

    # Setup Database
    database = None
    with open(local_database, 'r+', encoding='utf-8-sig') as db:
        database = js.load(db)['database']
        for data_entry in database:
            if 'ID' not in data_entry.keys():
                print('ERROR! A card has not been assigned an ID. This is crucial! Please fix this immediately!')
            if 'name' not in data_entry.keys():
                print('ERROR! A card has not been assigned a name. This is crucial! Please fix this immediately!')
            if 'description' not in data_entry.keys():
                data_entry['description'] = ''
            if 'color' not in data_entry.keys():
                if 'ID' in data_entry.keys():
                    id_color = data_entry['ID'][0]
                    data_entry['color'] = 'red' if id_color == 'R' else 'purple' if id_color == 'P' else 'blue' if id_color == 'B' else 'green' if id_color == 'G' else 'black' if id_color == 'S' else 'red'
                    print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no color assigned and was assumed the color' + data_entry['color'] + ' due to its ID.')
                else:
                    data_entry['color'] = 'red'
                    print('ERROR! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no color assigned, but the color could also not be assumed! Please fix this immediately!')
            if 'level' not in data_entry.keys():
                data_entry['level'] = '1'
                print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no level assigned. It was set to 1.')
            if 'card_type' not in data_entry.keys():
                if 'space' in data_entry.keys() or 'strength' in data_entry.keys() or 'health' in data_entry.keys():
                    data_entry['card_type'] = 'unit'
                    print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no card type' +
                          ' assigned and was assumed to be a unit. If it is a leader, please change this in the ' +
                          'database!')
                else:
                    data_entry['card_type'] = 'magic'
                    print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no card type assigned. It was assumed to be <MAGIC>.')
            if 'cost' not in data_entry.keys():
                data_entry['cost'] = '0'
                print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no cost assigned. It was set to 0.')
            if 'throwaway_cost' not in data_entry.keys():
                data_entry['throwaway_cost'] = '0'
                print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no throwaway cost assigned. It was set to 0.')
            if 'space' not in data_entry.keys():
                data_entry['space'] = '0'
                if data_entry['card_type'] != 'magic':
                    print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no space assigned. It was set to 0.')
            if 'strength' not in data_entry.keys():
                data_entry['strength'] = '0'
                if data_entry['card_type'] != 'magic':
                    print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no strength assigned. It was set to 0.')
            if 'health' not in data_entry.keys():
                data_entry['health'] = '0'
                if data_entry['card_type'] != 'magic':
                    print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no health assigned. It was set to 0.')
            if 'spell_speed' not in data_entry.keys():
                data_entry['spell_speed'] = '0'
                if data_entry['card_type'] == 'magic':
                    print('WARNING! Card ' + data_entry['name'] + ' (ID: ' + data_entry['ID'] + ') had no spell speed assigned. It was set to 0.')

    file_path_string = tkinter.filedialog.askopenfilename(title='Select Decklist Text File')
    start_time = datetime.datetime.now()
    singletons = []
    used_ids = []
    counts = {}

    # Read Deck File
    with open(file_path_string, 'r+', encoding='utf-8-sig') as deck_file:
        for line in deck_file:
            linedata = line.split()
            card_name = ''
            count = 1
            if len(linedata) > 1:
                card_name = ' '.join(linedata[1:])
                count = int(linedata[0])
            elif len(linedata) > 0:
                if linedata[0] == '#LEADERS':
                    continue
                card_name = linedata[0]

            json = {}
            if card_name != '':
                for entry in database:
                    if entry['ID'] == card_name or entry['name'] == card_name:
                        json = entry
                        break

                if json != {} and json['ID'] not in used_ids:
                    used_ids.append(json['ID'])
                    counts[json['ID']] = count
                    singletons.append((card_name, json))
                    print('Deck contains ' + json['name'])
                elif json != {}:
                    counts[json['ID']] += count

    # Generate Images
    current_cards = 0
    max_cards = len(singletons)

    print("Starting generation of " + str(max_cards) + " different cards. This can take up to a few minutes.")

    paths = {}

    for card_name, json in singletons:
        is_unit_card = json['card_type'] == 'leader' or json['card_type'] == 'unit'
        if is_unit_card:
            _, _, path = json_to_unit_card(json['name'], json['description'], json['color'], json['level'],
                                           json['cost'], json['throwaway_cost'], json['space'], json['strength'],
                                           json['health'])
            paths[json['ID']] = path
        else:
            _, _, path = json_to_card(json['name'], ('[Spell Speed ' + json['spell_speed'] + ']\n' if json['card_type'] == 'magic' else '') + json['description'],
                                      json['color'], json['level'], json['cost'], json['throwaway_cost'])
            paths[json['ID']] = path

        current_cards += 1
        print(str(int((float(current_cards)/float(max_cards)*100))) + "%")

    # Create the Atlas
    print("Creating Atlas...")

    if not os.path.exists(output_directory):
        os.mkdir(output_directory)

    img = Image.new('RGB', (6500, 7350))
    row = 0
    column = 0

    for entry in used_ids:
        count = counts[entry]
        path = paths[entry]
        current_image = Image.open(path)
        current_image = current_image.resize((650, 1050))
        copies_left = count
        print("ATLAS: Adding " + str(count) + " copies of " + entry + ".")

        while copies_left > 0:
            img.paste(current_image, (row*650, column*1050))
            copies_left -= 1
            row += 1
            if row > 10:
                row = 0
                column += 1

    img.save(output_directory + "atlas_" + time.strftime("%Y_%m_%d-%H_%M_%S") + ".png")
    end_time = datetime.datetime.now()
    duration = (end_time - start_time).total_seconds()
    print('Completed in ' + str(duration) + "s.")

    # TODO: Open target directory
    # TODO: Leader Decks
    # TODO: Card Backs

    if os.path.exists(temp_image_dir):
        shutil.rmtree(temp_image_dir)


def card_background_color(color):
    if color == 0 or color == 'red':  # red
        return 204, 102, 102
    elif color == 1 or color == 'purple':  # purple
        return 141, 109, 174
    elif color == 2 or color == 'green':  # green
        return 153, 204, 153
    elif color == 3 or color == 'blue':  # blue
        return 102, 169, 204
    else:  # black
        return 54, 54, 66


def placeholder_image(name):
    img_identifier = randrange(sys.maxsize - 1)
    output_dir = temp_image_dir + str(img_identifier)
    while os.path.exists(output_dir):
        img_identifier = randrange(sys.maxsize - 1)
        output_dir = temp_image_dir + "/" + str(img_identifier)

    os.mkdir(output_dir)
    name = name.replace(',', ' ').replace('/', ' ') + " anime"
    downloader.download(name, limit=1, output_dir=output_dir, adult_filter_off=False, force_replace=False,
                        timeout=60, verbose=False)
    redos = 3
    while len(os.listdir(output_dir + "\\" + name)) <= 0 < redos:
        downloader.download(name, limit=1, output_dir=output_dir, adult_filter_off=False, force_replace=False,
                            timeout=60, verbose=False)
        redos -= 1

    if len(os.listdir(output_dir + "\\" + name)) <= 0:
        print('WARNING: For Card ' + name + ', there could not be found a fitting image. It will proceed not to have an image.')
        return '_ERROR_'
    else:
        return output_dir + "\\" + name + "\\" + os.listdir(output_dir + "\\" + name)[0]


def set_frame_color(img, color):
    new_img = []
    r, g, b = color
    for item in img.getdata():
        if item[0] == item[1] == item[2] and item[0] in list(range(180, 256)):
            dif = 256 - item[0]
            new_img.append((r - dif, g - dif, b - dif))
        else:
            new_img.append(item)

    return new_img


def json_to_card(name, description, color, level, cost, throwaway_cost, show=False, save=True):
    img = Image.open(path_base_card)
    img = img.convert('RGB')
    path = card_image_dir + name + ".png"

    # Recoloring background / white parts
    c = card_background_color(color)
    img.putdata(set_frame_color(img, c))

    # Placeholder card sprite
    placeholder = placeholder_image(name)
    if placeholder != '_ERROR_':
        placeholder = Image.open(placeholder)
        w, h = placeholder.size
        cw, ch = card_sprite_size
        wpercent = (cw / w)
        th = int((float(h)) * float(wpercent))
        placeholder = placeholder.resize((cw, th))
        placeholder = placeholder.crop((0, inner_card_sprite_offset, cw, inner_card_sprite_offset + ch))
        img.paste(placeholder, (int(750 - cw / 2), 310))

    # Text
    I1 = ImageDraw.Draw(img)
    I1.text((100, 100), name, font=name_font, fill='black')
    I1.text((300, 1075), str(level), font=details_font, fill=c)
    I1.text((620, 1075), str(cost), font=details_font, fill=c)
    I1.text((725, 1950), str(throwaway_cost), font=name_font, fill='white')
    I1.text((75, 1200), description, font=text_font, fill='white')

    if show:
        img.show()

    if save:
        img.save(path)

    return img, I1, path


def json_to_unit_card(name, description, color, level, cost, throwaway_cost, space, strength, health, show=False, save=True):
    img, I1, path = json_to_card(name, description, color, level, cost, throwaway_cost, save=False)
    c = card_background_color(color)

    additionals = Image.open(path_unit_additionals)
    colored_additionals = additionals.copy()
    colored_additionals.putdata(set_frame_color(additionals, c))
    img.paste(colored_additionals, (0, 0), additionals)

    # Text
    I1 = ImageDraw.Draw(img)
    I1.text((925, 1075), str(space), font=details_font, fill=c)
    I1.text((1130, 1925), str(strength) + " / " + str(health), font=stats_font, fill='white')

    if show:
        img.show()

    if save:
        img.save(path)

    return img, I1, path


json_to_face()
