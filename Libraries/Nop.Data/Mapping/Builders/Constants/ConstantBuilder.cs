using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Constant;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Builders.Constants
{
    public partial class ConstantBuilder: NopEntityBuilder<Constant>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table

                .WithColumn(nameof(Constant.Id)).AsInt32().PrimaryKey().NotNullable()
                .WithColumn(nameof(Constant.GroupName)).AsString(30).NotNullable()
                .WithColumn(nameof(Constant.Code)).AsString(30).Nullable()
                .WithColumn(nameof(Constant.ArabicName)).AsString(256).NotNullable()
                .WithColumn(nameof(Constant.EnglishName)).AsString(256).NotNullable()
                .WithColumn(nameof(Constant.EmirateId)).AsString(30).Nullable()
                .WithColumn(nameof(Constant.CityId)).AsInt32().Nullable()
                .WithColumn(nameof(Constant.CountryGroup)).AsString(30).Nullable();
        }

        #endregion
    }
}
