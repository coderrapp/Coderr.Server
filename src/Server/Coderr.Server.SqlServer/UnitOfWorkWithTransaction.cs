﻿using System;
using System.Data;
using System.Data.SqlClient;
using Griffin.Data;
using log4net;

namespace Coderr.Server.SqlServer
{
    /// <summary>
    ///     Required for background jobs which uses
    /// </summary>
    public class UnitOfWorkWithTransaction : IAdoNetUnitOfWork
    {
        private ILog _logger = LogManager.GetLogger(typeof(UnitOfWorkWithTransaction));
        private SqlCommand _lastCommand;


        public UnitOfWorkWithTransaction(SqlTransaction transaction)
        {
            Transaction = transaction;
        }

        public SqlTransaction Transaction { get; private set; }

        public void Dispose()
        {
            if (Transaction == null)
                return;

            if (_lastCommand != null)
                _logger.Debug($"Rolling back {GetHashCode()}, last command: {_lastCommand.CommandText}");
            else
                _logger.Info($"Rolling back {GetHashCode()}");

            var connection = Transaction.Connection;
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;

            connection?.Dispose();
            _logger.Info("Rolled back " + GetHashCode());
        }

        public void SaveChanges()
        {
            // Already committed.
            // some scenarios requires early SaveChanges
            // to prevent dead locks. 
            // when there is time, find and eliminate the reason of the deadlocks :(
            if (Transaction == null)
                return;

            var connection = Transaction.Connection;
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
            connection.Dispose();
        }

        public IDbCommand CreateCommand()
        {
            var cmd = Transaction.Connection.CreateCommand();
            cmd.Transaction = Transaction;
            _lastCommand = cmd;
            return cmd;
        }

        public void Execute(string sql, object parameters)
        {
            throw new NotSupportedException();
        }
    }
}