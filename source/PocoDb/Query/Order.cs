using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using PocoDb.Utility;

namespace PocoDb.Query
{
    public enum Dir
    {
        Asc,
        Desc
    }

    public class Order<TModel>
    {
        private readonly IList<string> _orderConditions = new List<string>(); 

        private Order()
        {}

        internal IList<string> OrderConditions
        {
            get { return _orderConditions; }
        }

        public static Order<TModel> By<TProperty>(Expression<Func<TModel, TProperty>> column, Dir direction = Dir.Asc)
        {
            var order = new Order<TModel>();
            order.AddOrderCondition(column, direction);

            return order;
        }

        public Order<TModel> ThenBy<TProperty>(Expression<Func<TModel, TProperty>> column, Dir direction = Dir.Asc)
        {
            AddOrderCondition(column, direction);
            return this;
        }

        private void AddOrderCondition<TProperty>(Expression<Func<TModel, TProperty>> column, Dir direction)
        {
            string columnName = ModelUtility.GetColumnName(column);
            _orderConditions.Add(columnName + " " + direction);
        }
    }
}
