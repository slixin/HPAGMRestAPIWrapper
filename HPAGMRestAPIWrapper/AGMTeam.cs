using System;
using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    ///  Team collection class
    /// </summary>
    public class AGMTeams : AGMEntityCollection<AGMTeam>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMTeams(AGMConnection connection)
            : base(connection)
        {            
        }

        #region public methods
        #endregion
    }

    /// <summary>
    /// requirement class
    /// </summary>
    public class AGMTeam : AGMEntity
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
