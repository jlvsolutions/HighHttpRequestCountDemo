# HighHttpRequestCountDemo Sandbox
A console application which can host different strategies to perform a large number of http requests.
## Summary
This solution contains a .NET 8 Web API and a console client app 
for sandboxing ideas related to sending http requests.
The client it'self starts the web api and then proceeds to 
demonstrate different possible strategies for handling the need to send 
large numbers of HTTP requests to an APIwithout the angst of port 
exhaustion or other issues.
###
Additional demos are simple to add to the PerformDemo method.  Currently there
are only two demos added with a third coming soon.
### A Note
This code is not intended to be polished for a production environment, but
to provide the basics for proof of concepts/ideas.
###
Currently there are only two http strategies, a Semaphore and a TPL,
A third to possibly come shortly.