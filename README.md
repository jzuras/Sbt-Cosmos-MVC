# SoftballTech-Cosmos (MVC version)

This is a learning project for myself, and it is not expected to be useful to the world at large. However, my LI post discusses some speed bumps that I encountered that might be interesting for others just learning about Cosmos or MVC. 

For this project, I modified my previous Cosmos EF Core project to move from Razor Pages to MVC. (Please see the ReadMe in the original repo for more info about the website itself.)

This transition required reexamining how data gets to the View. I write about the issues in my LinkedIn post (see the end of this ReadMe for a link).

I ran into an issue attempting to create a new Cosmos DB Container for this project. Doing so would have exceeded the limits of the free version, so this project shares data with the prior EF Core version.

As with the others, this version is running on Azure, and can be found [here](https://sbt-cosmos-mvc.azurewebsites.net/).

The LinkedIn discussion post is [https://www.linkedin.com/feed/update/urn:li:share:7137479278622912514/]().
