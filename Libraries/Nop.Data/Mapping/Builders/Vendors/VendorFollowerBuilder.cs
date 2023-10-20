using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Builders.Vendors
{
    public class VendorFollowerBuilder : NopEntityBuilder<VendorFollower>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
               //.WithColumn(nameof(VendorFollower.Id)).AsInt32().NotNullable()
               .WithColumn(nameof(VendorFollower.CustomerId)).AsInt32().NotNullable()
               .WithColumn(nameof(VendorFollower.VendorId)).AsInt32().NotNullable()
               .WithColumn(nameof(VendorFollower.Unfollowed)).AsBoolean().NotNullable()
               .WithColumn(nameof(VendorFollower.UnFollowOnUtc)).AsBoolean().Nullable();
       

        }
    }
}
