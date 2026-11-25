import './ComponentsGrid.scss';
const COLUMNS = [
  { key: 'parentName', label: 'PARENTNAME' },
  { key: 'componentName', label: 'COMPONENTNAME' },
  { key: 'partNumber', label: 'PARTNUMBER' },
  { key: 'title', label: 'TITLE' },
  { key: 'quantity', label: 'QUANTITY' },
  { key: 'type', label: 'TYPE' },
  { key: 'item', label: 'ITEM' },
  { key: 'material', label: 'MATERIAL' }
];

const PART_KEYS = new Set(['partNumber', 'title', 'type', 'item', 'material']);

function displayValue(columnKey, row) {
  const value = row[columnKey];
  if (columnKey === 'quantity') {
    return value ?? '';
  }

  if (value === null || value === undefined || value === '') {
    return PART_KEYS.has(columnKey) ? '-' : '';
  }

  return value;
}

export default function ComponentsGrid({ rows, loading }) {
  return (
    <section className="grid-panel">
      <div className="grid-scroll">
        <table className="components-grid">
          <thead>
            <tr>
              {COLUMNS.map((column) => (
                <th key={column.key}>{column.label}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={COLUMNS.length} className="grid-status">Loading components...</td>
              </tr>
            )}
            {!loading && rows.length === 0 && (
              <tr>
                <td colSpan={COLUMNS.length} className="grid-status">&nbsp;</td>
              </tr>
            )}
            {!loading && rows.map((row, index) => (
              <tr key={`${row.parentName}-${row.componentName}-${index}`}>
                {COLUMNS.map((column) => (
                  <td key={column.key}>{displayValue(column.key, row)}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
