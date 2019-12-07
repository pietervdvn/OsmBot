# OsmBot
A C# program which automates a few OSM tasks, mainly conflating GRB and OSM

## How to use

0) If JOSM is open, close it
1) Run the program with 'dotnet run'; it'll say that the server is running on port 8111 (`cd OsmBot/OsmBot; dotnet run`)
2) Open the GRB import website. Zoom around, hit the 'Filter GRB' button and 'Export GRB'. The server mimics JOSM remote, but will save the GRB data to 'GRB0.osm', 'GRB1.osm', ... instead
3) Once you are satisfied with the regions downloaded, kill the server by returning to the terminal and hitting 'Ctrl+C'
4) Restart with 'dotnet run`. The program will detect the GRB[0-9]*.osm files and start confloting them.
5) When the program stops, a bunch of .osc files will be in the directory
6) Open them _all_ with JOSM, e.g. by dragging them all at the same time into JOSM.
7) Each changeset will be loaded as a layer. Select them all, rightclick and hit `merge`. (If merge is not visible, enable 'advanced mode' in settings)
8) Hit upload with a decent commit message
9) Remove the `GRB.osm`-files. As long as they are present, the program will try to conflate them again
10) Close JOSM and repeat
