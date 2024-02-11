Converter for StarCraft 1 aiscripts made by PyAI to the Asc3 format, which is much more easily readable and writable by hand. There already is a way to convert Asc3 scripts to PyAI but not vice-versa, until now.
Just drag and drop the PyAI script over the executable.


Will do everything that you would expect it to do when converting between the two formats:

-Removes all commas and the first pair of parentheses

-Removes spaces in block names

-Supports multiple scripts in the same file

-Comments out PyAI headers and adds the Asc3 header ("script_name" and "script_id"). This way you can just uncomment the PyAI headers after you import and convert the script in PyAI

It should also support all aise commands. 
After the conversion the script should be ready to be converted back with no issue.
