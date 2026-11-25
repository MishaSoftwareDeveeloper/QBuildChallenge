import './PartInformation.scss';
export default function PartInformation({
  path,
  currentPart,
  populated,
  loading,
  error,
  onPopulate
}) {
  return (
    <section className="info-panel">
      <div className="info-lines">
        <div className="parent-child-part">
          Parent Child Part: {path || ''}
        </div>
        <div className="current-part">
          Current Part: {currentPart || ''}
        </div>
      </div>
      <div className="info-actions">
        <button
          type="button"
          className="populate-button"
          onClick={onPopulate}
          disabled={populated || loading}
        >
          {loading ? 'Loading...' : 'Populate Data In Tree'}
        </button>
        {error && <div className="status-error">{error}</div>}
      </div>
    </section>
  );
}
