using System;
using System.Collections.Generic;
using System.Linq;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    ///  BuildInstance collection class
    /// </summary>
    public class AGMBuildInstances : AGMEntityCollection<AGMBuildInstance>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMBuildInstances(AGMConnection connection)
            : base(connection)
        {            
        }

        #region public methods
        #endregion
    }

    /// <summary>
    /// requirement class
    /// </summary>
    public class AGMBuildInstance : AGMEntity
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
