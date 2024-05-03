# Crypto Cart


Crypto Cart is an ecommerce prototype engineered to incorporate advanced technologies for an elevated digital shopping experience. At its core, Crypto Cart integrates machine learning (ML), database management, and cryptocurrency payment systems to redefine ecommerce standards and enhance operational efficiency.

Machine learning algorithms are deployed within Crypto Cart to optimize user engagement and satisfaction. Specifically, ML algorithms are employed for two key functions: product recommendation and customer churn prediction. These algorithms analyze user behavior and historical data to generate personalized product recommendations and forecast customer churn patterns, thereby maximizing user retention and satisfaction.

Backing Crypto Cart's operations is a robust MySQL database management system. This database stores critical information including product catalogs, user profiles, and transaction histories. Leveraging MySQL ensures efficient data storage, retrieval, and management, bolstering the platform's scalability and operational integrity.

In line with emerging trends in digital finance, Crypto Cart integrates a secure cryptocurrency payment gateway, facilitated by NowPayments.io. This integration enables users to conduct transactions using various cryptocurrencies, expanding payment options beyond traditional fiat currencies. By embracing cryptocurrency payments, Crypto Cart underscores its commitment to innovation and meets the evolving demands of digital commerce.

In summary, Crypto Cart represents a paradigm shift in ecommerce, leveraging advanced technologies such as machine learning, robust database management, and cryptocurrency payments to deliver unparalleled user experiences and drive operational excellence in the digital retail landscape.

# Technologies

- API - C#, .Net Core
- Database - MySql
- UI - CSHTML, CSS and Javascript
- Payment Gateway - [NowPayments](https://nowpayments.io/)

# Setup

- Change connection string in ```appsettings.json```.
- Run ```Update-Database``` command in Package Manager Console.
- Check if all the tables are present in Database after ```Update-Database``` Command execution.
- Run the project Create a User with Admin Access. 
