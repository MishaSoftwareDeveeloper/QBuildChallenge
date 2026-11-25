import { useEffect, useState } from 'react';
import './BomTreeNode.scss';

export default function BomTreeNode({ node, selectedId, onSelect, expandRoot }) {
  const hasChildren = Array.isArray(node.children) && node.children.length > 0;
  const [expanded, setExpanded] = useState(Boolean(expandRoot) || node.level === 1);
  const selected = selectedId === node.id;

  useEffect(() => {
    if (expandRoot || node.level === 1) {
      setExpanded(true);
    }
  }, [expandRoot, node.level]);

  const toggle = (event) => {
    event.stopPropagation();
    if (hasChildren) {
      setExpanded((value) => !value);
    }
  };

  return (
    <div className="tree-node">
      <button
        type="button"
        className={`tree-row${selected ? ' selected' : ''}`}
        onClick={() => onSelect(node)}
        style={{ paddingLeft: `${8 + (node.level - 1) * 16}px` }}
      >
        <span
          className={`tree-toggle ${hasChildren ? (expanded ? 'expanded' : 'collapsed') : 'leaf'}`}
          onClick={toggle}
          aria-hidden="true"
        />
        <span className="tree-label">{node.name}</span>
      </button>
      {hasChildren && expanded && (
        <div className="tree-children">
          {node.children.map((child) => (
            <BomTreeNode
              key={child.id}
              node={child}
              selectedId={selectedId}
              onSelect={onSelect}
            />
          ))}
        </div>
      )}
    </div>
  );
}
