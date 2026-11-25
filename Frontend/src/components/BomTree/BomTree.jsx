import BomTreeNode from './BomTreeNode.jsx';
import './BomTree.scss';

export default function BomTree({ tree, selectedId, onSelect, populated }) {
  return (
    <aside className="tree-panel">
      <div className="tree-scroll">
        {!populated && <div className="tree-placeholder">Tree is not populated.</div>}
        {populated && tree.map((node) => (
          <BomTreeNode
            key={node.id}
            node={node}
            selectedId={selectedId}
            onSelect={onSelect}
            expandRoot
          />
        ))}
      </div>
    </aside>
  );
}
