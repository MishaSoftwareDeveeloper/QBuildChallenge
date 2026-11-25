import Header from './components/Header/Header.jsx';
import BomTree from './components/BomTree/BomTree.jsx';
import PartInformation from './components/PartInformation/PartInformation.jsx';
import ComponentsGrid from './components/ComponentsGrid/ComponentsGrid.jsx';
import { useBom } from './hooks/useBom.js';

export default function App() {
  const {
    tree,
    selectedNode,
    selectedPath,
    components,
    treeLoading,
    gridLoading,
    error,
    populated,
    populateTree,
    selectNode
  } = useBom();

  return (
    <div className="app-shell">
      <Header />
      <div className="top-section">
        <BomTree
          tree={tree}
          selectedId={selectedNode?.id}
          onSelect={selectNode}
          populated={populated}
        />
        <PartInformation
          path={selectedPath}
          currentPart={selectedNode?.name}
          populated={populated}
          loading={treeLoading}
          error={error}
          onPopulate={populateTree}
        />
      </div>
      <div className="section-separator" />
      <ComponentsGrid rows={components} loading={gridLoading} />
    </div>
  );
}
