import { useCallback, useRef, useState } from 'react';
import { getBomTree, getComponents } from '../services/bomService';

function findNode(nodes, predicate) {
  for (const node of nodes) {
    if (predicate(node)) {
      return node;
    }

    const nested = findNode(node.children || [], predicate);
    if (nested) {
      return nested;
    }
  }

  return null;
}

function buildPath(node) {
  if (!node?.id) {
    return '';
  }

  return node.id.split('/').filter(Boolean).join('\\');
}

export function useBom() {
  const [tree, setTree] = useState([]);
  const [selectedNode, setSelectedNode] = useState(null);
  const [components, setComponents] = useState([]);
  const [treeLoading, setTreeLoading] = useState(false);
  const [gridLoading, setGridLoading] = useState(false);
  const [error, setError] = useState('');
  const [populated, setPopulated] = useState(false);
  const requestIdRef = useRef(0);
  const selectedNodeIdRef = useRef(null);

  const loadComponents = useCallback(async (node) => {
    const requestId = ++requestIdRef.current;
    setGridLoading(true);
    setError('');

    try {
      const rows = await getComponents(node.name);
      if (requestId !== requestIdRef.current) {
        return;
      }

      setComponents(Array.isArray(rows) ? rows : []);
    } catch (err) {
      if (requestId !== requestIdRef.current) {
        return;
      }

      setComponents([]);
      setError(err.message || 'Failed to load components.');
    } finally {
      if (requestId === requestIdRef.current) {
        setGridLoading(false);
      }
    }
  }, []);

  const populateTree = useCallback(async () => {
    if (populated || treeLoading) {
      return;
    }

    setTreeLoading(true);
    setError('');

    try {
      const data = await getBomTree();
      const nodes = Array.isArray(data) ? data : [];
      setTree(nodes);
      setPopulated(true);

      const root = findNode(nodes, (node) => node.name?.toUpperCase() === 'VALVE') || nodes[0] || null;
      setSelectedNode(root);
      if (root) {
        await loadComponents(root);
      } else {
        setComponents([]);
      }
    } catch (err) {
      setTree([]);
      setSelectedNode(null);
      setComponents([]);
      setPopulated(false);
      setError(err.message || 'Failed to populate the tree.');
    } finally {
      setTreeLoading(false);
    }
  }, [loadComponents, populated, treeLoading]);

  const selectNode = useCallback((node) => {
    if (!node || !populated) {
      return;
    }

    if (selectedNodeIdRef.current === node.id) {
      return;
    }
  
    selectedNodeIdRef.current = node.id;

    setSelectedNode(node);
    loadComponents(node);
  }, [loadComponents, populated]);

  return {
    tree,
    selectedNode,
    selectedPath: buildPath(selectedNode),
    components,
    treeLoading,
    gridLoading,
    error,
    populated,
    populateTree,
    selectNode
  };
}
