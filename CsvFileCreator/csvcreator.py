from pynput import keyboard
import time
import random
from pandas import DataFrame
import os
import struct

# create pipe server between C# and python code
f = open(r'\\.\pipe\NPtest', 'wb', 0)

# create folder and files where the program path runs
tdate = time.strftime("%y-%m-%d")
dir_location = os.path.dirname(__file__)
file_path = os.path.join(dir_location, './database/' + tdate)

if not os.path.exists(file_path):
    os.makedirs(file_path)

filename = time.strftime("%H-%M-%S")
temp_path = os.path.join(dir_location, './temp/')  # add folder specified location.

if not os.path.exists(temp_path):  # control path existence. if not create it.
    os.makedirs(temp_path)

file = open(os.path.join(temp_path, 'tempadate.txt'), "w")  # open file with os without permission error
file.write(filename)
file.close()

angle = {'Point Que': [], 'Real Distance': [], 'Edge 1 Distance': [], 'Edge 2 Distance': [], 'Angle 1 Distance': [],
         'Angle 2 Distance': []}  # create dictionary for pandas dataframe
temp_time = time.strftime("%H.%M.%S")
stop_export = False  # while condition related to add values to dictionary
dataFrame_index = 1  # index value for dataframe

df = DataFrame(angle, columns=['Point Que', 'Real Distance', 'Edge 1 Distance', 'Edge 2 Distance', 'Angle 1 Distance',
                               'Angle 2 Distance'])
df.to_csv(os.path.join(file_path, filename + '.csv'), index=False, header=True, encoding='utf-8')
file_situation = 'Open'
arr = bytes(file_situation, 'utf-8')
print(struct.pack('I', len(file_situation)) + arr)
f.write(struct.pack('I', len(file_situation)) + arr)
f.seek(0)  # file descriptor to beginning of file

def createfile(angle):  # creating csv file according to count of dataframe values

    global filename, df
    df = DataFrame(angle,
                   columns=['Point Que', 'Real Distance', 'Edge 1 Distance', 'Edge 2 Distance', 'Angle 1 Distance',
                            'Angle 2 Distance'])
    df.to_csv(os.path.join(file_path, filename + '.csv'), index=False, header=True, encoding='utf-8')


def start(key):  # create csv file start condition also, it ends first listener to create file.

    if key == keyboard.Key.f6:
        print('Exporting start...')
        return False  # stop first listener for on_press


def stop(key):  # program stop condition. it actives when listener running condition breaks.

    global file_situation
    try:
        if key == keyboard.Key.f7:
            file_situation = 'End'
            arr1 = bytes(file_situation, 'utf-8')
            f.write(struct.pack('I', len(file_situation)) + arr1)
            f.seek(0)
            print('Exporting stopped...')
            return False  # when listener return false program stops.

    except ValueError:  # adds for if array lengths not equal. if the program does not stop,press the f7 key again

        print(len(angle['Point Que']), len(angle['Real Distance']), len(angle['Edge 1 Distance']),
              len(angle['Edge 2 Distance']), len(angle['Angle 1 Distance']), len(angle['Angle 2 Distance']))


with keyboard.Listener(on_press=start) as listener:  # create key listener for starting.
    listener.join()  # wait f6 to start

with keyboard.Listener(
        on_press=stop) as listener:  # If the start condition is provided, the program continues to run from here and
    # also creates a listener running to follow the status of the stop condition.
    line_count = 0
    line_counter = 0
    while not stop_export:  # loop continues until breaks it.
        for key in angle:  # create dictionary values for pandas dataframe
            if key == 'Point Que':
                angle[key] += [dataFrame_index]
                dataFrame_index += 1
            elif key == 'Real Distance':
                angle[key] += [random.randint(1240, 1400)]
            elif key == 'Edge 1 Distance':
                angle[key] += [round(random.uniform(617, 700), 13)]
            elif key == 'Edge 2 Distance':
                angle[key] += [round(random.uniform(-1500, -1200), 13)]
            elif key == 'Angle 1 Distance':
                angle[key] += [400]
            else:
                angle[key] += [round(random.randint(300, 500))]

        time.sleep(0.01)
        line_count += 1

        if line_count == 10:  # when condition approved it calls createfile function.
            createfile(angle)
            line_counter += 1
            print(line_counter*10, "Line Exporting...")
            line_count = 0

        # press f7 and  go stop function.
        if not listener.running:  # listener tells us whether listener works actively
            break  # if listener turns false then loop breaks
