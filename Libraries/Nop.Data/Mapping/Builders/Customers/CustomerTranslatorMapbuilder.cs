using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Builders.Customers
{
    public partial class CustomerTranslatorMapbuilder : NopEntityBuilder<CustomerTranslationMapping>
    {
        public CustomerTranslatorMapbuilder() { 
        }
        public override void MapEntity(CreateTableExpressionBuilder table)
        {

          table
            .WithColumn(nameof(CustomerTranslationMapping.IsTranslated)).AsBoolean().Nullable()
            .WithColumn(nameof(CustomerTranslationMapping.EntityName)).AsString(400).NotNullable();
        }
      
    }
}
