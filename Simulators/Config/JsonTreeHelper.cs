using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Simulators.Config
{
    public static class JsonTreeHelper
    {
        /// <summary>
        /// Populates the TreeView with nodes based on the provided JSON root node.
        /// </summary>
        public static void PopulateTree(TreeView treeView, JsonNode rootNode)
        {
            treeView.Nodes.Clear();

            var root = new TreeNode("Root")
            {
                Tag = rootNode,
                Text = FormatNodeLabel(rootNode, "Root")
            };

            treeView.Nodes.Add(root);
            AddChildNodes(root, rootNode);
            treeView.ExpandAll();
        }

        private static void AddChildNodes(TreeNode parentNode, JsonNode node)
        {
            if (node is JsonObject obj)
            {
                foreach (var kvp in obj)
                {
                    if (kvp.Value == null)
                        continue;

                    var child = new TreeNode(FormatNodeLabel(kvp.Value, kvp.Key))
                    {
                        Tag = kvp.Value
                    };
                    parentNode.Nodes.Add(child);
                    AddChildNodes(child, kvp.Value);
                }
            }
            else if (node is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    var item = arr[i];
                    if (item == null)
                        continue;

                    var child = new TreeNode(FormatNodeLabel(item, $"[{i}]"))
                    {
                        Tag = item
                    };
                    parentNode.Nodes.Add(child);
                    AddChildNodes(child, item);
                }
            }
        }

        /// <summary>
        /// Creates a readable label for any node (key + value or array index + value).
        /// </summary>
        public static string FormatNodeLabel(JsonNode node, string? key = null)
        {
            if (node is JsonValue val)
            {
                string valueStr = val.ToJsonString();
                return key != null ? $"{key}: {valueStr}" : valueStr;
            }
            else if (node is JsonArray)
            {
                return key != null ? $"{key} [Array]" : "[Array]";
            }
            else if (node is JsonObject)
            {
                return key != null ? $"{key} {{Object}}" : "{Object}";
            }

            return key ?? "null";
        }
    }
}
