# Melissa Updater
This is a CLI application allowing the user to update their Melissa applications/data utilizing the Melissa Releases API.

For more information on the Melissa Releases API, please see: <https://docs.melissa.com/cloud-api/melissa-releases/melissa-releases-index.html>

For more information on the Melissa Updater, please see: <https://docs.melissa.com/software/melissa-updater/melissa-updater-index.html>

## Table of Contents
- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Verbs](#verbs)
- [Parameters](#parameters)
- [Sample Commands](#sample-commands)
- [Update Schedule](#update-schedule)

----------------------------------------

## Requirements
- Windows: Windows 11 64-bit or newer
- Linux: Ubuntu 22.04 LTS
- Dotnet Core SDK 8.0 or newer
----------------------------------------

## Getting Started
This download link will get you a copy of the Melissa Updater to use on your machine. Download Melissa Updater here:
- Windows: <https://releases.melissadata.net/Download/Library/WINDOWS/NET/ANY/latest/MelissaUpdater.exe>
- Linux: <https://releases.melissadata.net/Download/Library/LINUX/NET/ANY/latest/MelissaUpdater>

For .NET7 compatibility, download this [Windows](https://releases.melissadata.net/Download/Library/WINDOWS/NET/ANY/2025.02/MelissaUpdater.exe/?&TAG=NET7) or [Linux](https://releases.melissadata.net/Download/Library/LINUX/NET/ANY/2025.02/MelissaUpdater/?&TAG=NET7) version.

----------------------------------------

## Verbs
|Verb       |Description              |
|-----------|-------------------------|
|file       |Download a single file.  |
|manifest   |Download all files within a product.|
|product    |Download a ZIP file of a specific product.|
|verify     |Verify a file or a folder path.|
|help       |Display more information on a specific command.|
|version    |Display version information.|

----------------------------------------

## Parameters

### File
|Short   |Long                 |Description                    |
|--------|---------------------|-------------------------------|
|-a		|--architecture       |The specific architecture for the binary file (64BIT, 32BIT, ANY).       |
|-c		|--compiler           |The specific compiler for the binary file <br>(ACC3, ANY, C, COM, DLL, GCC32, GCC34, GCC41, GCC46, GCC48, GCC83,<br> JAVA, MSSQL, NET, PERL, PHP, PHP7, PLSQL, PYTHON, RUBY,<br> SSIS2005, SSIS2008, SSIS2012, SSIS2014, SSIS2016, SSIS2017, SSIS2019, WS12, WS6, XLC12, XLC6).   					|
|-d		|--dry_run            |Simulate the process without modifying any files.                   	|
|-f	    |--force              |Force the download and overwrite existing file(s).                 		|
|-l		|--license            |The valid Melissa license string for the product you wish to download. |
|-n		|--filename           |The filename to download.                                              |
|-o		|--os                 |The specific operating system for the binary file<br> (AIX, ANY, HPUX_IT, HPUX_PA, LINUX, SOLARIS, WINDOWS, ZLINUX).            					|
|-q     |--quiet              |Run the program in quiet mode without console output except for errors.|
|-r     |--release_version    |The release version (YYYY.MM, YYYY.Q#, LATEST) for the product you wish to download (e.g. "2023.01" or "2023.Q1" or "LATEST").    		    |
|-t     |--target_directory   |The target directory where to place the downloaded file(s).<br> If not specified, the default is the current directory. 					|
|-w     |--working_directory  |The working directory where to temporarily stage downloaded file(s) before moving into the target directory.        					|
|-x     |--callback           |Action command for the next script or process to run (Windows only).    	|
|-y     |--type	              |The specific file type to be downloaded (BINARY, DATA, INTERFACE).    	|
|       |--help	              |Display the help screen.    	|
|       |--version	          |Display version information.    	|


### Manifest

You can retrieve a list of available manifest names using the Melissa Releases API: <https://releases.melissadata.net/ManifestList/YYYY.MM>

Alternatively, you can view it in a browser: <https://releases.melissadata.net/Browse>

|Short   |Long                 |Description                    |
|--------|---------------------|-------------------------------|
|-d		 |--dry_run            |Simulate the process without modifying any files.|
|-f	     |--force              |Force the download and overwrite existing file(s).                 		|
|-i	     |--index              |Retrieve and list all files in a manifest.                 		|
|-l		 |--license            |The valid Melissa license string for the product you wish to download. |
|-m      |--map                |The map file with your custom file structure for downloaded file(s).|
|-p      |--product            |The manifest name to be downloaded.          |
|-q      |--quiet              |Run the program in quiet mode without console output except for errors.|
|-r      |--release_version    |The release version (YYYY.MM, YYYY.Q#, LATEST) for the product you wish to download (e.g. "2023.01" or "2023.Q1" or "LATEST").    		    |
|-t      |--target_directory   |The target directory where to place the downloaded file(s). If not specified, the default is the current directory. 					|
|-w      |--working_directory  |The working directory where to temporarily stage downloaded file(s) before moving into the target directory.       					|
|-x      |--callback           |Action command for the next script or process to run (Windows only).    	|
|        |--help	           |Display the help screen.    	|
|        |--version	           |Display version information.    	|

### Product

|Short   |Long                 |Description                    |
|--------|---------------------|-------------------------------|
|-d		 |--dry_run            |Simulate the process without modifying any files.|
|-f	     |--force              |Force the download and overwrite existing file(s).                 		|
|-l		 |--license            |The valid Melissa license string for the product you wish to download. |
|-p      |--product            |The product name to be downloaded.          |
|-q      |--quiet              |Run the program in quiet mode without console output except for errors.|
|-r      |--release_version    |The release version (YYYY.MM, YYYY.Q#, LATEST) for the product you wish to download (e.g. "2023.01" or "2023.Q1" or "LATEST").    		    |
|-t      |--target_directory   |The target directory where to place the downloaded file(s). If not specified, the default is the current directory. 					|
|-w      |--working_directory  |The working directory where to temporarily stage downloaded file(s) before moving into the target directory.       					|
|-x      |--callback           |Action command for the next script or process to run (Windows only).    	|
|        |--help	           |Display the help screen.    	|
|        |--version	           |Display version information.    	|

### Verify
|Short   |Long                 |Description                    |
|--------|---------------------|-------------------------------|
|-p	     |--path               |The file or folder path that you wish to verify.                 		|
|-q      |--quiet              |Run the program in quiet mode without console output except for errors.|
|-x      |--callback           |Action command for the next script or process to run (Windows only).    	|
|        |--help	           |Display the help screen.    	|
|        |--version	           |Display version information.    	|
----------------------------------------

## Sample Commands
### Windows version
#### File
* Interface
    ```
    .\MelissaUpdater.exe file -n "mdPhone_cSharpCode.cs" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -o "ANY" -c "NET" -a "ANY" -y "INTERFACE" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\"
    ```

    Download to a working directory:
    ```
    .\MelissaUpdater.exe file -n "mdPhone_cSharpCode.cs" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -o "ANY" -c "NET" -a "ANY" -y "INTERFACE" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\" -w "\YOUR\PATH\TO\WORKING\DIRECTORY"
    ```

* Binary
    ```
    .\MelissaUpdater.exe file -n "mdPhone.dll" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -o "WINDOWS" -c "DLL" -a "64BIT" -y "BINARY" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\"
    ```

    Download to a working directory:
    ```
    .\MelissaUpdater.exe file -n "mdPhone.dll" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -o "WINDOWS" -c "DLL" -a "64BIT" -y "BINARY" -t  "\YOUR\PATH\TO\TARGET\DIRECTORY\" -w "\YOUR\PATH\TO\WORKING\DIRECTORY"
    ```

* Data

    For data files, flags for type (-y), operating system (-o), architecture (-a), and compiler (-c) are not mandatory. You can use either of the commands below:
    ```
    .\MelissaUpdater.exe file -n "mdPhone.dat" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\"
    ```
    ```
    .\MelissaUpdater.exe file -n "mdPhone.dat" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -y "DATA" -o "ANY" -c "ANY" -a "ANY" -t  "\YOUR\PATH\TO\TARGET\DIRECTORY\"
    ```

    Download to a working directory:
    ```
    .\MelissaUpdater.exe file -n "mdPhone.dat" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\" -w "\YOUR\PATH\TO\WORKING\DIRECTORY"
    ```
    ```
    .\MelissaUpdater.exe file -n "mdPhone.dat" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -y "DATA" -o "ANY" -c "ANY" -a "ANY" -t  "\YOUR\PATH\TO\TARGET\DIRECTORY\" -w "\YOUR\PATH\TO\WORKING\DIRECTORY"
    ```


#### Manifest
There are 2 ways to use Manifest option:
* Without Map directory: All files in the product will be downloaded into the same folder.

    ```
    .\MelissaUpdater.exe manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l  "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\"
    ```

    Download to a working directory:
    ```
    .\MelissaUpdater.exe manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l  "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\" -w "\YOUR\PATH\TO\WORKING\DIRECTORY"
    ```

* With Map directory: All files in the product will be downloaded into folder/subfolders based on the structure specified in .map files. You can pass in either absolute or relative map file path. Map files structure examples can be found in MelissaUpdater\Maps folder.
	* Absolute path:
	```
    .\MelissaUpdater.exe manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\" -m "C:\YOUR\PATH\TO\MAP\DIRECTORY\Maps\dq_phone_data.map"
    ```

	* Relative path:
	```
    .\MelissaUpdater.exe manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\" -m ".\Maps\dq_phone_data.map"
    ```

    * Download to a working directory:
    ```
    .\MelissaUpdater.exe manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\" -m "C:\YOUR\PATH\TO\MAP\DIRECTORY\Maps\dq_phone_data.map" -w "\YOUR\PATH\TO\WORKING\DIRECTORY"
    ```
    ```
    .\MelissaUpdater.exe manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\" -m ".\Maps\dq_phone_data.map" -w "/YOUR/PATH/TO/WORKING/DIRECTORY"
    ```

#### Product
```
.\MelissaUpdater.exe product -p "Melissa_product_name" -r "RELEASE_VERSION" -l  "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\"
```
Download to a working directory:
```
.\MelissaUpdater.exe product -p "Melissa_product_name" -r "RELEASE_VERSION" -l  "REPLACE_WITH_LICENSE_STRING" -t "\YOUR\PATH\TO\TARGET\DIRECTORY\" -w "\YOUR\PATH\TO\WORKING\DIRECTORY"
```

#### Verify
* A folder
    ```
    .\MelissaUpdater.exe verify -p "C:\YOUR\PATH\TO\DIRECTORY"
    ```

* A specific file
    ```
    .\MelissaUpdater.exe verify -p "C:\YOUR\PATH\TO\DIRECTORY\Filename.txt"
    ```


### Linux version
#### File
* Interface
    ```
    ./MelissaUpdater file -n "mdPhone_cSharpCode.cs" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -o "ANY" -c "NET" -a "ANY" -y "INTERFACE" -t "/YOUR/PATH/TO/DIRECTORY"
    ```

    Download to a working directory:
    ```
    ./MelissaUpdater file -n "mdPhone_cSharpCode.cs" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -o "ANY" -c "NET" -a "ANY" -y "INTERFACE" -t "/YOUR/PATH/TO/DIRECTORY" -w "/YOUR/PATH/TO/WORKING/DIRECTORY"
    ```


* Binary
    ```
    ./MelissaUpdater file -n "mdPhone.dll" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -o "WINDOWS" -c "DLL" -a "64BIT" -y "BINARY" -t "/YOUR/PATH/TO/DIRECTORY"
    ```

    Download to a working directory:
    ```
    ./MelissaUpdater file -n "mdPhone.dll" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -o "WINDOWS" -c "DLL" -a "64BIT" -y "BINARY" -t "/YOUR/PATH/TO/DIRECTORY" -w "/YOUR/PATH/TO/WORKING/DIRECTORY"
    ```

* Data

    For data files, flags for type (-y), operating system (-o), architecture (-a), and compiler (-c) are not mandatory. You can use either of the commands below:
    ```
    ./MelissaUpdater file -n "mdPhone.dat" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/DIRECTORY"
    ```
    ```
    ./MelissaUpdater file -n "mdPhone.dat" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -y "DATA" -o "ANY" -c "ANY" -a "ANY" -t "/YOUR/PATH/TO/DIRECTORY"
    ```

    Download to a working directory:
    ```
    ./MelissaUpdater file -n "mdPhone.dat" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/TARGET/DIRECTORY" -w "/YOUR/PATH/TO/WORKING/DIRECTORY"
    ```
    ```
    ./MelissaUpdater file -n "mdPhone.dat" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -y "DATA" -o "ANY" -c "ANY" -a "ANY" -t  "/YOUR/PATH/TO/DIRECTORY" -w "/YOUR/PATH/TO/WORKING/DIRECTORY"
    ```


#### Manifest
There are 2 ways to use Manifest option:
* Without Map directory:

    All files in the product will be downloaded into the same folder.

    ```
    ./MelissaUpdater manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/DIRECTORY"
    ```
    Download to a working directory:
    ```
    ./MelissaUpdater manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/TARGET/DIRECTORY" -w "/YOUR/PATH/TO/WORKING/DIRECTORY"
    ```

* With Map directory: All files in the product will be downloaded into folder/subfolders based on the structure specified in .map files. You can pass in either absolute or relative map file path. Map files structure examples can be found MelissaUpdater/Maps folder.
	* Absolute path:
	```
    ./MelissaUpdater manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/DIRECTORY" -m "/YOUR/PATH/TO/MAP/DIRECTORY/Maps/Melissa_product_name.map"
    ```

	* Relative path:
	```
    ./MelissaUpdater manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/DIRECTORY" -m "./Maps/Melissa_product_name.map"
    ```
    * Download to a working directory:
    ```
    ./MelissaUpdater manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/DIRECTORY" -m "/YOUR/PATH/TO/MAP/DIRECTORY/Maps/Melissa_product_name.map" -w "/YOUR/PATH/TO/WORKING/DIRECTORY"
    ```
    ```
    ./MelissaUpdater manifest -p "Melissa_product_name" -r "RELEASE_VERSION" -l "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/DIRECTORY" -m "./Maps/Melissa_product_name.map" -w "/YOUR/PATH/TO/DIRECTORY/WORKING/DIRECTORY"
    ```

#### Product
```
./MelissaUpdater product -p "Melissa_product_name" -r "RELEASE_VERSION" -l  "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/TARGET/DIRECTORY"
```
Download to a working directory:
```
./MelissaUpdater product -p "Melissa_product_name" -r "RELEASE_VERSION" -l  "REPLACE_WITH_LICENSE_STRING" -t "/YOUR/PATH/TO/TARGET/DIRECTORY" -w "/YOUR/PATH/TO/WORKING/DIRECTORY"
```

#### Verify
* A folder
    ```
    ./MelissaUpdater verify -p "/YOUR/PATH/TO/DIRECTORY"
    ```

* A specific file
    ```
    ./MelissaUpdater verify -p "/YOUR/PATH/TO/DIRECTORY/Filename.txt"
    ```


----------------------------------------

## Update Schedule
|Monthly |Bimonthly |Quarterly|
|--------|----------|---------|
|01	     |B6	    |Q1       |
|02	     |B1	    |Q1       |
|03	     |B1	    |Q1       |
|04	     |B2	    |Q2       |
|05	     |B2	    |Q2       |
|06	     |B3	    |Q2       |
|07	     |B3	    |Q3       |
|08	     |B4	    |Q3       |
|09	     |B4	    |Q3       |
|10	     |B5	    |Q4       |
|11	     |B5	    |Q4       |
|12	     |B6	    |Q4       |





