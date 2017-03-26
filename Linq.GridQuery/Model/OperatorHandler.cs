using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Linq.GridQuery.Model
{
    public class OperatorHandler
    {
        public OperatorHandler(
            Func<Type, Type, Expression, Expression, Expression> expressionFactory)
        {
            ExpressionFactory = expressionFactory;
        }

        public OperatorHandler(
            Func<Type, Type, Expression, Expression, Expression> expressionFactory,
            Func<Type, Type, Type> propTypeConverter)
        {
            ExpressionFactory = expressionFactory;
            PropTypeConverter = propTypeConverter;
        }

        public OperatorHandler(
                       Func<Type, Type, Expression, Expression, Expression> expressionFactory,
                       bool skipNullCheck)
        {
            ExpressionFactory = expressionFactory;
            SkipNullCheck = skipNullCheck;
        }

        public OperatorHandler(
                    Func<Type, Type, Expression, Expression, Expression> expressionFactory,
                    Func<Type, Type, Type> propTypeConverter,
                    bool skipNullCheck)
        {
            ExpressionFactory = expressionFactory;
            PropTypeConverter = propTypeConverter;
            SkipNullCheck = skipNullCheck;
        }

        /// <summary>
        /// Mmethod used produce filter expression for this kind of operand 
        /// </summary>
        public Func<Type, Type, Expression, Expression, Expression> ExpressionFactory { get; private set; }

        /// <summary>
        /// Optional method used to determine, which type of value the operator expects, when it is not 1 to 1 with prop type
        /// </summary>
        public Func<Type, Type, Type> PropTypeConverter { get; private set; }

        public bool SkipNullCheck { get; private set; } = false;
    }
}
