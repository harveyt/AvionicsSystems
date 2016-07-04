﻿/*****************************************************************************
 * The MIT License (MIT)
 * 
 * Copyright (c) 2016 MOARdV
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * 
 ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace AvionicsSystems
{
    /// <summary>
    /// MASComponent encapsulates the action(s) attached to a particular prop.
    /// It is intended to contain multiple sub-nodes, each of which describe an
    /// action and the parameters required to trigger that action.  Note that
    /// some actions are considered one-shot, meaning they are triggered at
    /// Start() and the node isn't referenced again.
    /// </summary>
    internal class MASComponent : InternalModule
    {
        private List<IMASAction> actions = new List<IMASAction>();

        /// <summary>
        /// Configure this module and its children.
        /// </summary>
        public void Start()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                return;
            }

            try
            {
                MASFlightComputer comp = MASFlightComputer.Instance(internalProp.part);
                if (comp == null)
                {
                    throw new ArgumentNullException("Unable to find ASFlightComputer in part - please check part configs");
                }

                ConfigNode moduleConfig = Utility.GetPropModuleConfigNode(internalProp.propName, moduleID);
                if (moduleConfig == null)
                {
                    throw new ArgumentNullException("No ConfigNode found!");
                }

                int persistentNodes = 0, transientNodes = 0;
                ConfigNode[] actionNodes = moduleConfig.GetNodes();
                for (int i = 0; i < actionNodes.Length; ++i)
                {
                    IMASAction action = Utility.CreateAction(actionNodes[i], internalProp, comp);
                    if (action != null)
                    {
                        if (action.Persistent())
                        {
                            ++persistentNodes;
                            actions.Add(action);
                        }
                        else
                        {
                            ++transientNodes;
                        }
                    }
                }

                Utility.LogMessage(this, "Configuration complete in prop #{0} ({1}): {2} persistent nodes created, {3} transient nodes", internalProp.propID, internalProp.propName, persistentNodes, transientNodes);
            }
            catch (Exception e)
            {
                Utility.LogErrorMessage(this, "Failed to configure prop #{0} ({1})", internalProp.propID, internalProp.propName);
                Utility.LogErrorMessage(this, e.ToString());
            }
        }

        /// <summary>
        /// Release resources used by this module's children.
        /// </summary>
        public void OnDestroy()
        {
            try
            {
                MASFlightComputer comp = MASFlightComputer.Instance(internalProp.part);
                Utility.LogMessage(this, "OnDestroy()");
                for (int i = 0; i < actions.Count; ++i)
                {
                    actions[i].ReleaseResources(comp);
                }
            }
            catch { }
        }
    }
}
