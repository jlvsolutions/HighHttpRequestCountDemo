# HighHttpRequestCountDemo Sandbox
A console application which can host different strategies to perform a large number of http requests.
## Summary
This solution contains a .NET 8 Web API and with a friendly console client app 
for sandboxing ideas related to sending http requests.
The client it'self starts the web api and then proceeds to 
demonstrate different possible strategies for handling the need to send 
large numbers of HTTP requests to an API without the angst of port 
exhaustion, DNS changes or other issues.
###
Additional demos/strategies are simple to add to the client.
Currently there are four http strategies, a SemaphoreSlim, TPL, ConcurrentQueue, 
and BlockingCollecion. 
### A Note
This solution contains code which is not intended to be polished as in a 
production environment, but rather to provide the basic environment 
for creating proof of concepts or testing ideas.
### A Note To Run
After cloning or opening from GitHub with VS, be sure to set the console project as the startup project.
It launches the Web API for you.

