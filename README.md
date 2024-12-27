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
Currently there are three http strategies, a SemaphoreSlim, TPL, and queueing. 
### A Note
This solution contains code which is not intended to be polished as in a 
production environment, but rather to provide the basic environment 
for creating proof of concepts or testing ideas.

