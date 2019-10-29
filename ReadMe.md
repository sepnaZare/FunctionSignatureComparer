# FunctionSignatureComparer#

A Tool for showing commits of a java-based git repository that add parameters to functions; 
    
## System requirements
* Linux or macOS or Windows
* git
* .Net Core 2.0.3

## How to run

### 1. Clone the Repository 
Clone this repository by this command : 
	git clone https://github.com/sepnaZare/FunctionSignatureComparer

### 2. Run the dll File
In command line, change your directory to PublicOutput folder that is available in this path of the repository: 


	cd ~/FunctionSignatureComparer/FunctionSignatureComparer/bin/Release/PublishOutput

then You can run the FunctionSignatureComparer.dll file with the following command:


	dotnet FunctionSignatureComparer.dll "$inputJavaRepositoriesPath"

The $inputGitRepositoriesPath is the absolute path of Java-Based git repository that you want to check its situation about adding parameters. This path must have .git directory so be sure that you clone the repository.

## Results

The results are saved in the $inputGitRepositoriesPath folder path in Results.csv file.

## Test Cases That Have Been Tested #

    * I've tested this tool with three popular Java Github repositories and lseditkconfig that is one of the academic java projects of prof. Sarah Nadi.
        *    MPAndroidChart : https://github.com/PhilJay/MPAndroidChart
        *    Retrofit : https://github.com/square/retrofit
        *    OkHttp : https://github.com/square/okhttp/
        *    lseditkconfig : https://github.com/snadi/lseditkconfig/commits/master
    * All results of these test cases are accessible in git repository of the tool in ./Results folder with their owen-Name.csv
    
    
    