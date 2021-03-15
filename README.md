# Real-time_data_flow_graph

This project is designed to read the relevant data from the csv file and dynamically present the real-time data flow chart to the user 
with C # windows form. Although it is a little far from being a comprehensive and usable user interface, it can be made suitable 
by adding the necessary components. If you already have a csv file for which you need to import the data, you will not need the python code, 
but for this you will need to change the design of the lastfilepath class in the C # code and remove the pipe server.

Basically, the working logic of the project is as follows. A csv file is needed to draw the necessary data in the chart. 
This csv file is generated with python code. The path of the generated csv file should be written in the lastfilepath 
class in the C # win-form interface. After these operations, the C # win-form application is run as a pipe server. 
The connection with the python client is expected by pressing the start button in the interface reflected on the screen.
When the Python code is run, the pathi of the csv file to be generated is caught in the C # application. By pressing the F6 key in python code, 
the values ​​produced by the python ​​are written to the csv file. Values ​​written to the csv file start to be drawn instantly
in the C # win-form interface simultaneously. To finish writing the value of the csv file and graph, press f7 in the python code.
The drawn graphic is removed from the C # win-form interface and pipe server connection is expected again. 
Thus, it is aimed to dynamically draw new csv file values ​​to be created with python without restarting the project.

Project Details

