# ODataExpressionVisitor
Implementation of the ExpressionVisitor in the OData package.

This project handles the incoming OData query object and uses a custom visitor to create an Expression. 
So you can feed the expression to any data source and the possibilty to create multiple layers.
This method avoids blackbox magic of the OData package. 

Run the example and try the OData queries like:
- http://localhost:5000/api/example?$filter=Name eq 'Name 1'
- http://localhost:5000/api/example?$filter=startsWith(Description,'ads')