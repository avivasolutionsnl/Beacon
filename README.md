<!-- markdownlint-disable MD041 -->
![CI](https://github.com/avivasolutionsnl/Beacon/workflows/CI/badge.svg?branch=master)

# Beacon

A monitoring tool for Azure DevOps and Teamcity that uses a Delcom USB LED or Shelly Bulb light to notify your teams.

![Chocolate](./Images/Screenshot.png)

## How to install

The easiest deployment mechanism is to install [Chocolatey](https://chocolatey.org/) and run the following command-line to install the [Chocolate package](https://chocolatey.org/packages/beacon):

    choco install beacon

Then run `beacon` and observe the command-line arguments.

## Example usage
### Azure Devops
Using a public Azure Devops project and running continuously for definition id (ie. build) #1 and #2:

    beacon azuredevops --url https://dev.azure.com/my-public-project --project MyProject --builds=1 2

Or, using a personal access token (e.g. 1234567890ab, see [here](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page) for how to obtain a token) and for all definitions:

    beacon azuredevops --url https://dev.azure.com/my-public-project --project MyProject --builds=* --token 1234567890ab

### TeamCity
Using a named TeamCity account and running continuously:

    beacon teamcity --url=http://yourteamcity.com --username=username --password=password --builds=build_id_1 build_id_2 etc

Or, alternatively using TeamCity guest access, running only once and with verbose logging:

    beacon teamcity --url=http://yourteamcity.com --guestaccess --runonce --verbose --builds=build_id_1 build_id_2 etc

## Using a Shelly Bulb
First install your Shelly Bulb following the installation [instructions](https://shelly.cloud/documents/user_guide/shelly_bulb.pdf).
After your bulb is installed, look up the device IP address of it under "Settings > Device Information", e.g. 10.25.31.27.

Now use the obtained IP to connect Shelly:

    beacon azuredevops --url https://dev.azure.com/avivasolutions-public --project Nyxie --builds 10 --device shelly --shellyurl http://10.25.31.27

## Backlog

* Support for configuring the RGB values, flash mode and power level of the LED.
* Support for including all TeamCity projects under a certain project node, including more advanced filtering rules.
* Deployment as a Windows Service.

## Why another

My original idea was triggered by [TeamFlash](https://github.com/Readify/TeamFlash), a prototype implementation made by Readify. Unfortunately, the quality of that code was way below anything I can work with, so I planned to contribute a big list of improvements as well as more configuration options. However, it took me several emails, tweets and DMs and a month of patience just to get a [single pull request](https://github.com/Readify/TeamFlash/pull/16) accepted. So I see no point in working with a fork. Instead, I decided to take their code as a starting point and revive the product.
