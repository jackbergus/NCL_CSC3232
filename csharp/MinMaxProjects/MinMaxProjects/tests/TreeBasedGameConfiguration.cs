using System;
using System.Collections.Generic;

namespace MinMaxProjects.tests
{
    /// <summary>
    /// Example of a class keeping track of the whole evolution of a game
    /// </summary>
    public class TreeBasedGameConfiguration<T>
    {
        /// <summary>
        /// In this simplistic approach, all we are interested is the sequence of actions that the player performed.
        /// The reason for that is the following: we are going to provide a tree-bounded approach, rather than 
        /// exploiting 
        /// </summary>
        public List<T> actionsPerformed;

        public TreeBasedGameConfiguration()
        {
            actionsPerformed = new List<T>();
        }

        /// <summary>
        /// Returns a new game configuration, where the action provided as an argument is performed as a last action.
        /// Id est, it represents the state evolution from the current configuration, to the configuration where the
        /// players did something at all.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TreeBasedGameConfiguration<T> appendAction(T action)
        {
            var conf = new TreeBasedGameConfiguration<T>();
            foreach (var x in actionsPerformed) conf.actionsPerformed.Add(x);
            conf.actionsPerformed.Add(action);
            return conf;
        }

        public override string ToString()
        {
            string result = "{";
            foreach (var x in actionsPerformed) result += (x + ", ");
            result += "}";
            return result;
        }
    }

}
