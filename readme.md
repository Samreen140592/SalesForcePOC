Thank you for your interest in Beringer Technology Group.
I'd like you to provide a code sample that will show off your ability to develop in Azure.

Here's the requirements for your Function App:
- Written in c#, and using the latest version of .NET
- Timer triggerd, set to run every 30 minutes
- Query the SalesForce API, and return data in a JSON format.
        - Here is the SalesForece API documentation:  https://www.integrate.io/blog/salesforce-rest-api-integration/
        - If you are having trouble creating a Salesforece demo environment, you can try using these creds for API access
  instanceURL:  https://orgfarm-33d88c2b67-dev-ed.develop.lightning.force.com/lightning/page/home
  clientId = "3MVG9rZjd7MXFdLgf7.rnByfpxc.qdoJ0dksu077pUATWQy9QJxGoq1lPSRTxLB80VVrprp2j62WB48vdfYKf"; 
  clientSecret = "87AC8561BC7AEA44E52AE8EB04FE2F079DDCB25BE010E282F474432B94CBE12D";  
- Store the JSON data in a service bus queue called DevTest.
        - Here is the connection string:  Endpoint=sb://dsc00000075-datasynccloud-dev.servicebus.windows.net/;SharedAccessKeyName=DevTest;SharedAccessKey=BTqdrsBxiBawz5AOiJQTE8asCKp5RghRs+ASbC2CjmA=
        - Here is the queue URL:  https://dsc00000075-datasynccloud-dev.servicebus.windows.net/devtest
- Document the function app in a Readme.MD file

Please clone this branch, and upload your completed Visual Studio project to this repository.  I should be able to clone your branch, then compile and run your project.
Reach out to me with any questions.
Good luck :)

Rob Hess
rhess@beringer.net
