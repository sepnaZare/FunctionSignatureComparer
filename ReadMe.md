# FunctionSignatureComparer#

A Tool for showing commits of a java-based git repository that add parameters to functions; 
    
## System requirements
* Linux or macOS or Windows
* git
* .Net Core 3.0.0

## How to run

### 1. Clone the Repository 
Clone this repository by this command : 
	git clone https://github.com/sepnaZare/FunctionSignatureComparer

### 2. Build the project
In command line, change your directory to FunctionSignatureComparer/FunctionSignatureComparer folder that has FunctionSignatureComparer.csproj: 

Run the following command to clean the last output to prepoare it to build:
	
	dotnet clean
	
then run this command to build the project:
	dotnet build 

after running build command, you get the message "Build Succeeded" and it shows a path that contains FunctionSignatureComparer.dll. It is usually created at ./FunctionSignatureComparer/bin/Debug/netcoreapp3.0/FunctionSignatureComparer.dll.

### 3. Run the FunctionSignatureComparer.dll file:

After buiding successfully, run this command to execute the dll file:

	dotnet ./FunctionSignatureComparer/bin/Debug/netcoreapp3.0/FunctionSignatureComparer.dll "$inputJavaRepositoriesPath"

The $inputGitRepositoriesPath is the absolute path of Java-Based git repository that you want to check its situation about adding parameters. This path must have .git directory so be sure that you clone the repository.

## Results

The results are saved in the $inputGitRepositoriesPath folder path in Results.csv file.

## Test Cases That Have Been Tested #

    * I've tested this tool with three popular Java Github repositories that have mentioned below.
        *    MPAndroidChart : https://github.com/PhilJay/MPAndroidChart
        *    Retrofit : https://github.com/square/retrofit
        *    OkHttp : https://github.com/square/okhttp/
    * All results of these test cases are accessible in git repository of the tool in ./Results folder with their owen-Name.csv
    
    
    