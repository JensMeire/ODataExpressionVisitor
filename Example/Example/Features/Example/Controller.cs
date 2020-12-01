using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Utilities;

namespace Example.Features.Example
{
    public class Controller : ControllerBase
    {
        private List<ExampleAggregate> _data;

        public Controller()
        {
            _data = new List<ExampleAggregate>
            {
                new ExampleAggregate
                {
                    Age = 1,
                    Description = "asd",
                    Name = "Name 1",
                    NestedObject = new NestedObject
                    {
                        Description = "jlakdf"
                    },
                    NotFilterableProperty = "Secret"
                },
                new ExampleAggregate
                {
                    Age = 23,
                    Description = "adsgadhgjfaf",
                    Name = "Name 2",
                    NestedObject = new NestedObject
                    {
                        Description = "afksahdfkjsagfkjhdgfy"
                    },
                    NotFilterableProperty = "Secret"
                },
                new ExampleAggregate
                {
                    Age = 456,
                    Description = "rteioeutyrjhgjdfg",
                    Name = "Name 3",
                    NestedObject = new NestedObject
                    {
                        Description = "fhdjslfjdkhjfdgsfg"
                    },
                    NotFilterableProperty = "Secret"
                },
                new ExampleAggregate
                {
                    Age = 1,
                    Description = "erootiudfgufujdhgysd",
                    Name = "Name 4",
                    NestedObject = new NestedObject
                    {
                        Description = "aaaaaaaaaaaaaaaaashjdfgjhfgjdshf"
                    },
                    NotFilterableProperty = "Secret"
                }
            };
        }

        [HttpGet("api/example")]
        public IActionResult Get(ODataQueryOptions<ExampleModel> options)
        {
            if (options?.Filter?.FilterClause?.Expression == null)
                return Ok(_data);
            
            var visitor = new FilterVisitor<ExampleAggregate, ExampleColumn>(GetField, ColumnToProperty);
            try
            {
                var expression = options.Filter.FilterClause.Expression.Accept(visitor);
                var pred = Expression.Lambda<Func<ExampleAggregate, bool>>(expression, visitor.GetParameterExpression());
                var result = _data.AsQueryable().Where(pred);
                //Mapping to a model to expose
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
        
        public ExampleColumn? GetField(Type type, MemberInfo member)
        {
            if (type == typeof(ExampleModel))
            {
                return member.Name switch
                {
                    nameof(ExampleModel.Name) => ExampleColumn.Name,
                    nameof(ExampleModel.Description) => ExampleColumn.Description,
                    nameof(ExampleModel.Age) => ExampleColumn.Age,
                    _ => throw new NotImplementedException()
                };
            }

            if (type == typeof(NestedModel))
            {
                return member.Name switch
                {
                    nameof(NestedModel.Description) => ExampleColumn.NestedProperty,
                    _ => throw new NotImplementedException()
                };
            }
            throw new NotImplementedException();
        }
        
        
        private Expression<Func<ExampleAggregate, object>> ColumnToProperty(ExampleColumn? column)
        {
            return column switch
            {
                ExampleColumn.Description => i => i.Description,
                ExampleColumn.Age => i => i.Age,
                ExampleColumn.Name => i => i.Name,
                ExampleColumn.NestedProperty => i => i.NestedObject.Description,
                _ => null
            };
        }
    }
}