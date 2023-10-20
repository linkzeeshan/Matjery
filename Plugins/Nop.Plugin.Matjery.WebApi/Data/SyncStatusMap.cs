using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Matjery.WebApi.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Data
{
    public partial class SyncStatusMap : NopEntityBuilder<SyncStatus>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SyncStatus.ForceSyncDate)).AsDateTime().NotNullable();
        }

        #endregion
    }
}
