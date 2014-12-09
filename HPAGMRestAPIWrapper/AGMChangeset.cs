using System;
using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    ///  Changeset collection class
    /// </summary>
    public class AGMChangesets : AGMEntityCollection<AGMChangeset>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMChangesets(AGMConnection connection)
            : base(connection)
        {            
        }

        #region public methods
        #endregion
    }

    /// <summary>
    /// requirement class
    /// </summary>
    public class AGMChangeset : AGMEntity
    {
        #region private members
        #endregion       

        #region public Properties
        #endregion

        #region constructor
        #endregion

        #region public methods        
        #endregion

        #region private methods
        #endregion
    }
}
