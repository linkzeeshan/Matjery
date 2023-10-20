using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Sms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Builders.Sms
{
    public partial class QueuedSmsBuilder : NopEntityBuilder<QueuedSms>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(QueuedSms.Message)).AsString(500).NotNullable()
                .WithColumn(nameof(QueuedSms.PhoneNumber)).AsString(500).Nullable();
        }

        #endregion
    }
}
