using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Lookups;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Lookups
{
    public partial class LookupBuilder : NopEntityBuilder<Lookup>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Lookup.Type)).AsString(1000).Nullable()
                .WithColumn(nameof(Lookup.Value)).AsString(1000).Nullable()
                .WithColumn(nameof(Lookup.LanguageId)).AsInt32().Nullable()
                .WithColumn(nameof(Lookup.Sequence)).AsInt32().Nullable()
                .WithColumn(nameof(Lookup.IsActive)).AsBoolean().Nullable();
        }

        #endregion
    }
}
