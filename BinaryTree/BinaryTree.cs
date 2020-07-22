using System;
using System.Collections;
using System.Collections.Generic;

namespace BinaryTree
{
    public class BinaryTree<T> : IEnumerable<T>
    {
        /// <summary>
        /// Handles events for adding and removing elements
        /// </summary>
        /// <param name="sender">Instance of <see cref="BinaryTree<T>"/> that called the event</param>
        /// <param name="args">Arguments passed by sender for subscribers</param>
        public delegate void TreeEventHandler(object sender, TreeEventArgs<T> args);

        /// <summary>
        /// Event that should be called when new element is added
        /// </summary>
        public event TreeEventHandler ElementAdded;

        /// <summary>
        /// Event that should be called when element in tree is removed
        /// </summary>
        public event TreeEventHandler ElementRemoved;

        /// <summary>
        /// Defines how many elements tree contains
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Defines first element of tree
        /// </summary>
        public BinaryTreeNode<T> Root { get; private set; }

        /// <summary>
        /// Defines comparer
        /// </summary>
        private IComparer<T> Comparer { get; }

        /// <summary>
        /// Checks if type T implements <see cref="IComparable<T>"/>
        /// If it does: saves and uses as default comparer
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when T doesn't implement <see cref="IComparable<T>"</exception>
        public BinaryTree()
        {
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                Comparer = Comparer<T>.Default;
            else
                throw new ArgumentException("T doesn't implement IComparable<T>");
        }

        /// <summary>
        /// Creates instance of tree and saves custom comparer passed by parameter
        /// </summary>
        /// <param name="comparer"><see cref="IComparer<T>"/></param>
        public BinaryTree(IComparer<T> comparer)
        {
            Comparer = comparer ?? Comparer<T>.Default;
        }


        /// <summary>
        /// Adds element to the tree according to comparer
        /// </summary>
        /// <param name="item">Object that should be added in tree</param>
        /// <exception cref="ArgumentNullException">Thrown if parameter was null</exception>
        public void Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            BinaryTreeNode<T> node = new BinaryTreeNode<T>(item);

            if (Root == null)
            {
                Root = node;
            }
            else
            {
                BinaryTreeNode<T> current = Root;
                BinaryTreeNode<T> parent = Root;

                while (current != null)
                {
                    parent = current;
                    if (Comparer.Compare(item, current.Data) < 0)
                        current = current.Left;
                    else
                        current = current.Right;
                }

                if (Comparer.Compare(item, parent.Data) < 0)
                    parent.Left = node;
                else
                    parent.Right = node;
            }
            ++Count;

            ElementAdded?.Invoke(this, new TreeEventArgs<T>(item, "Element Added"));
        }

        /// <summary>
        /// Removes element from tree by its reference
        /// </summary>
        /// <param name="item">Object that should be removed from tree</param>
        /// <returns>True if element was deleted succesfully, false if element wasn't found in tree</returns>
        public bool Remove(T item)
        {
            if (Contains(item))
            {
                Root = Remove(Root, item);

                --Count;

                ElementRemoved?.Invoke(this, new TreeEventArgs<T>(item, "Element Removed"));

                return true;
            }
            return false;
        }

        private BinaryTreeNode<T> Remove(BinaryTreeNode<T> parent, T item)
        {
            if (Comparer.Compare(item, parent.Data) < 0) 
                parent.Left = Remove(parent.Left, item);
            else if (Comparer.Compare(item, parent.Data) > 0)
                parent.Right = Remove(parent.Right, item);
            // if value is same as parent's value, then this is the node to be deleted  
            else
            {
                // node with only one child or no child  
                if (parent.Left == null)
                    return parent.Right;
                else if (parent.Right == null)
                    return parent.Left;

                // node with two children: Get the inorder successor (smallest in the right subtree)  
                parent.Data = TreeMin(parent.Right);

                // Delete the inorder successor  
                parent.Right = Remove(parent.Right, parent.Data);
            }

            return parent;
        }

        private T TreeMin(BinaryTreeNode<T> node)
        {
            while (node.Left != null)
            {
                node = node.Left;
            }
            return node.Data;
        }


        /// <summary>
        /// Returns item with the highest value
        /// </summary>
        /// <returns>The element with highest value</returns>
        /// <exception cref="InvalidOperationException">Thrown if tree is empty</exception> 
        public T TreeMax()
        {
            if (Root is null)
                throw new InvalidOperationException("Tree is empty");

            BinaryTreeNode<T> current = Root;

            while (current.Right != null)
            {
                current = current.Right;
            }
            return current.Data;
        }

        /// <summary>
        /// Returns item with the lowest value
        /// </summary>
        /// <returns>The element with lowest value</returns>
        /// <exception cref="InvalidOperationException">Thrown if tree is empty</exception>
        public T TreeMin()
        {
            if (Root is null)
                throw new InvalidOperationException("Tree is empty");

            BinaryTreeNode<T> current = Root;

            while (current.Left != null)
            {
                current = current.Left;
            }
            return current.Data;
        }

        /// <summary>
        /// Checks if tree contains element by its reference
        /// </summary>
        /// <param name="item">Object that should (or not) be found in tree</param>
        /// <returns>True if tree contains item, false if it doesn't</returns>
        public bool Contains(T data)
        {
            if (data is null)
                return false;

            BinaryTreeNode<T> current = Root;

            while (current != null)
            {
                if (Comparer.Compare(data, current.Data) == 0)
                    return true;
                else if (Comparer.Compare(data, current.Data) < 0)
                    current = current.Left;
                else
                    current = current.Right;
            }

            return false;
        }

        /// <summary>
        /// Makes tree traversal
        /// </summary>
        /// <param name="traverseType"><see cref="TraverseType"></param>
        /// <returns>Sequense of elements of tree according to traverse type</returns>
        public IEnumerable<T> Traverse(TraverseType traverseType)
        {
            if (traverseType == TraverseType.InOrder)
                return (IEnumerable<T>)TraverseInOrder(Root).GetEnumerator();
            else if (traverseType == TraverseType.PreOrder)
                return (IEnumerable<T>)TraversePreOrder(Root).GetEnumerator();
            else
                return (IEnumerable<T>)TraversePostOrder(Root).GetEnumerator();
        }


        /// <summary>
        /// Makes in-order traverse
        /// Serves as a default <see cref="TraverseType"/> for tree
        /// </summary>
        /// <returns>Enumerator for iterations in foreach cycle</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)Traverse(TraverseType.InOrder);
        }

        /// <summary>
        /// Makes i n-order traverse
        /// Serves as a default <see cref="TraverseType"/> for tree
        /// </summary>
        /// <returns>Enumerator for iterations in foreach cycle</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> TraverseInOrder(BinaryTreeNode<T> current)
        {
            if (current != null)
            {
                foreach (var value in TraverseInOrder(current.Left))
                    yield return value;

                yield return current.Data;

                foreach (var value in TraverseInOrder(current.Right))
                    yield return value;
            }
        }

        public IEnumerable<T> TraversePreOrder(BinaryTreeNode<T> current)
        {
            if (current != null)
            {
                yield return current.Data;

                foreach (var value in TraversePreOrder(current.Left))
                    yield return value;


                foreach (var value in TraversePreOrder(current.Right))
                    yield return value;
            }
        }

        public IEnumerable<T> TraversePostOrder(BinaryTreeNode<T> current)
        {
            if (current != null)
            {
                foreach (var value in TraversePostOrder(current.Left))
                    yield return value;

                foreach (var value in TraversePostOrder(current.Right))
                    yield return value;

                yield return current.Data;
            }
        }
    }
}
