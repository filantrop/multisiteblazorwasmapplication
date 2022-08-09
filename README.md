# Blazor multi site
This project main goal is to create a solution with the base projects needed for a Blazor multi site with prepared configuration to add support for Openid/OAuth with auth0.
It can be used when you want a fast up and go with booth authentication and authorization enabled and multisite with different url domains.

One public Blazor wasm spa project and public .net core api project that uses different urls.
And one admin Blazor wasm spa project and admin .net core api project that also uses different urls.
The 4 projects is hosted in dockers with a ngninx reverse proxy infront.

There is a setup script to generate different artifacts based on a config file:
- Creating selfsigned certificate and adding them as thrusted to get https:// 
- Adding domains to the hosts file for testing to use your different environment domains in your browser
- Creating launchsettings.json based on the environments file
- Creating .env files to be used for docker-compose when building and running the services
- Creating static configuration file Domain/AppEnvironment.cs for the Blazor.wasm clients
- Generating different Dockerfiles for nginx and automatically including dependent on Environment to redirect www.your.domain to your.domain 

I will soon include all the steps to use the project.
