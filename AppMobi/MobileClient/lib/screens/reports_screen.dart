import 'package:flutter/material.dart';

import '../app_state.dart';
import '../formatters.dart';
import '../models.dart';
import '../widgets.dart';

class ReportsScreen extends StatefulWidget {
  const ReportsScreen({super.key, required this.state});

  final AppState state;

  @override
  State<ReportsScreen> createState() => _ReportsScreenState();
}

class _ReportsScreenState extends State<ReportsScreen> {
  String mode = 'day';
  String day = dateInput(DateTime.now());
  DateTime month = DateTime(DateTime.now().year, DateTime.now().month);
  String from = dateInput(DateTime.now());
  String to = dateInput(DateTime.now());
  int? branchId;
  bool loading = true;
  String? error;
  RevenueSummary? report;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      loading = true;
      error = null;
    });
    try {
      final data = switch (mode) {
        'month' => await widget.state.api.revenueMonth(
          month: monthInput(month),
          branchId: branchId,
        ),
        'range' => await widget.state.api.revenueRange(
          from: from,
          to: to,
          branchId: branchId,
        ),
        _ => await widget.state.api.revenueDay(date: day, branchId: branchId),
      };
      setState(() => report = data);
    } catch (exception) {
      setState(() => error = exception.toString());
    } finally {
      if (mounted) {
        setState(() => loading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          SegmentedButton<String>(
            segments: const [
              ButtonSegment(
                value: 'day',
                label: Text('Ngày'),
                icon: Icon(Icons.today_outlined),
              ),
              ButtonSegment(
                value: 'month',
                label: Text('Tháng'),
                icon: Icon(Icons.calendar_month_outlined),
              ),
              ButtonSegment(
                value: 'range',
                label: Text('Khoảng'),
                icon: Icon(Icons.date_range_outlined),
              ),
            ],
            selected: {mode},
            onSelectionChanged: (value) {
              setState(() => mode = value.first);
              _load();
            },
          ),
          const SizedBox(height: 12),
          BranchDropdown(
            branches: widget.state.branches,
            value: branchId,
            onChanged: (value) {
              setState(() => branchId = value);
              _load();
            },
          ),
          const SizedBox(height: 10),
          if (mode == 'day')
            _DateButton(
              label: 'Ngày báo cáo',
              value: day,
              onPicked: (value) {
                setState(() => day = value);
                _load();
              },
            )
          else if (mode == 'month')
            _MonthPicker(
              month: month,
              onChanged: (value) {
                setState(() => month = value);
                _load();
              },
            )
          else
            Row(
              children: [
                Expanded(
                  child: _DateButton(
                    label: 'Từ ngày',
                    value: from,
                    onPicked: (value) => setState(() => from = value),
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: _DateButton(
                    label: 'Đến ngày',
                    value: to,
                    onPicked: (value) => setState(() => to = value),
                  ),
                ),
              ],
            ),
          if (mode == 'range') ...[
            const SizedBox(height: 10),
            FilledButton.icon(
              onPressed: _load,
              icon: const Icon(Icons.filter_alt_outlined),
              label: const Text('Xem báo cáo'),
            ),
          ],
          const SizedBox(height: 14),
          if (loading)
            const SizedBox(height: 360, child: LoadingPane())
          else if (error != null)
            EmptyState(
              icon: Icons.error_outline,
              title: 'Không tải được báo cáo',
              message: error,
            )
          else if (report == null)
            const EmptyState(
              icon: Icons.bar_chart_outlined,
              title: 'Chưa có dữ liệu',
            )
          else
            _ReportBody(report: report!, mode: mode),
        ],
      ),
    );
  }
}

class _DateButton extends StatelessWidget {
  const _DateButton({
    required this.label,
    required this.value,
    required this.onPicked,
  });

  final String label;
  final String value;
  final ValueChanged<String> onPicked;

  @override
  Widget build(BuildContext context) {
    return OutlinedButton.icon(
      onPressed: () async {
        final current = DateTime.tryParse(value) ?? DateTime.now();
        final picked = await showDatePicker(
          context: context,
          initialDate: current,
          firstDate: DateTime(2020),
          lastDate: DateTime.now().add(const Duration(days: 366)),
        );
        if (picked != null) {
          onPicked(dateInput(picked));
        }
      },
      icon: const Icon(Icons.event_outlined),
      label: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: Theme.of(context).textTheme.labelSmall),
          Text(value),
        ],
      ),
    );
  }
}

class _MonthPicker extends StatelessWidget {
  const _MonthPicker({required this.month, required this.onChanged});

  final DateTime month;
  final ValueChanged<DateTime> onChanged;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        border: Border.all(color: Theme.of(context).colorScheme.outlineVariant),
        borderRadius: BorderRadius.circular(8),
        color: Colors.white,
      ),
      child: Row(
        children: [
          IconButton(
            tooltip: 'Tháng trước',
            onPressed: () => onChanged(DateTime(month.year, month.month - 1)),
            icon: const Icon(Icons.chevron_left),
          ),
          Expanded(
            child: Center(
              child: Text(
                monthInput(month),
                style: Theme.of(context).textTheme.titleMedium,
              ),
            ),
          ),
          IconButton(
            tooltip: 'Tháng sau',
            onPressed: () => onChanged(DateTime(month.year, month.month + 1)),
            icon: const Icon(Icons.chevron_right),
          ),
        ],
      ),
    );
  }
}

class _ReportBody extends StatelessWidget {
  const _ReportBody({required this.report, required this.mode});

  final RevenueSummary report;
  final String mode;

  @override
  Widget build(BuildContext context) {
    final buckets = mode == 'range' && report.monthly.length > 1
        ? report.monthly
              .map(
                (item) => (
                  label: item.month,
                  value: item.revenue,
                  count: item.orderCount,
                ),
              )
              .toList()
        : report.daily
              .map(
                (item) => (
                  label: displayDate(item.date),
                  value: item.revenue,
                  count: item.orderCount,
                ),
              )
              .toList();
    final maxRevenue = buckets
        .map((item) => item.value)
        .fold<double>(0, (max, value) => value > max ? value : max);
    final maxStatus = report.byStatus
        .map((item) => item.amount)
        .fold<double>(0, (max, value) => value > max ? value : max);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        LayoutBuilder(
          builder: (context, constraints) {
            final twoColumns = constraints.maxWidth > 560;
            return GridView.count(
              crossAxisCount: twoColumns ? 2 : 1,
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              mainAxisSpacing: 10,
              crossAxisSpacing: 10,
              childAspectRatio: twoColumns ? 3.2 : 3.8,
              children: [
                MetricTile(
                  icon: Icons.payments_outlined,
                  label: 'Doanh thu hoàn thành',
                  value: money(report.revenue),
                  color: Colors.green,
                ),
                MetricTile(
                  icon: Icons.receipt_long_outlined,
                  label: 'Tổng đơn',
                  value: '${report.orderCount}',
                ),
                MetricTile(
                  icon: Icons.pending_actions_outlined,
                  label: 'Chờ phê duyệt',
                  value: '${report.pendingApprovalCount}',
                  color: Colors.orange,
                ),
                MetricTile(
                  icon: Icons.cancel_outlined,
                  label: 'Đã hủy',
                  value: '${report.cancelledCount}',
                  color: Colors.red,
                ),
              ],
            );
          },
        ),
        SectionTitle(
          title: 'Khoảng thời gian',
          action: Text(
            '${displayDate(report.from)} - ${displayDate(report.to)}',
          ),
        ),
        if (buckets.isEmpty)
          const EmptyState(
            icon: Icons.insights_outlined,
            title: 'Chưa có doanh thu hoàn thành',
          )
        else
          ...buckets.map(
            (item) => RevenueBar(
              label: item.label,
              value: item.value,
              maxValue: maxRevenue,
              trailing: '${item.count} đơn',
            ),
          ),
        if (report.byStatus.isNotEmpty) ...[
          const SectionTitle(title: 'Theo trạng thái'),
          ...report.byStatus.map(
            (item) => RevenueBar(
              label: item.status,
              value: item.amount,
              maxValue: maxStatus,
              trailing: '${item.orderCount} đơn',
            ),
          ),
        ],
      ],
    );
  }
}
