import { Routes, Route } from 'react-router-dom'
import { AppShell } from '@/layouts/AppShell'
import { DashboardPage } from '@/pages/DashboardPage'
import { ScorecardsPage } from '@/pages/ScorecardsPage'
import { EditScorecardPage } from '@/pages/EditScorecardPage'
import { StaffPage } from '@/pages/StaffPage'
import { EditStaffPage } from '@/pages/EditStaffPage'
import { PermissionsPage } from '@/pages/PermissionsPage'
import { AuditLogPage } from '@/pages/AuditLogPage'
import { LoginPage } from '@/pages/LoginPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { ScoringPage } from '@/features/scoring/ScoringPage'
import { ReviewPage } from '@/features/review/ReviewPage'
import { EvaluationDetailPage } from '@/features/review/EvaluationDetailPage'
import { CalibrationPage } from '@/features/calibration/CalibrationPage'
import { CalibrationListBuilderPage } from '@/features/calibration/CalibrationListBuilderPage'
import { CalibrationItemsPage } from '@/features/calibration/CalibrationItemsPage'
import { CalibrationItemDetailPage } from '@/features/calibration/CalibrationItemDetailPage'
import { ReportsPage } from '@/features/reports/ReportsPage'

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<AppShell />}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/score" element={<ScoringPage />} />
        <Route path="/score/:evaluationId" element={<ScoringPage />} />
        <Route path="/review" element={<ReviewPage />} />
        <Route path="/review/:id" element={<EvaluationDetailPage />} />
        <Route path="/calibration" element={<CalibrationPage />} />
        <Route path="/calibration/:listId/builder" element={<CalibrationListBuilderPage />} />
        <Route path="/calibration/:listId/items" element={<CalibrationItemsPage />} />
        <Route path="/calibration/items/:itemId" element={<CalibrationItemDetailPage />} />
        <Route path="/reports" element={<ReportsPage />} />
        <Route path="/scorecards" element={<ScorecardsPage />} />
        <Route path="/scorecards/:id/edit" element={<EditScorecardPage />} />
        <Route path="/staff" element={<StaffPage />} />
        <Route path="/staff/:id" element={<EditStaffPage />} />
        <Route path="/settings/permissions" element={<PermissionsPage />} />
        <Route path="/audit-log" element={<AuditLogPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
