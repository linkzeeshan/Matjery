using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.BlackPoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Builders.BlackPoints
{
    public partial class BlackPointBuilder : NopEntityBuilder<BlackPoint>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
         
                .WithColumn(nameof(BlackPoint.Id)).AsInt32().PrimaryKey().NotNullable().Identity()
                .WithColumn(nameof(BlackPoint.Comment)).AsString(500).Nullable()
                .WithColumn(nameof(BlackPoint.AddedByCustomerId)).AsInt32().Nullable()
                .WithColumn(nameof(BlackPoint.AddedOnTypeId)).AsInt32().Nullable()
                .WithColumn(nameof(BlackPoint.Note)).AsString(500).Nullable()
                .WithColumn(nameof(BlackPoint.CreatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(BlackPoint.BlackPointStatus)).AsInt32().Nullable()
                .WithColumn(nameof(BlackPoint.VendorOrCustomerId)).AsInt32().Nullable();


        }

        #endregion
    }
}
