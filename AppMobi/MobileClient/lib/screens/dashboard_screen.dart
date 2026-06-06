import 'package:flutter/material.dart';

import '../app_state.dart';
import '../formatters.dart';
import '../models.dart';
import '../widgets.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key, required this.state});

  final AppState state;

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  bool loading = true;
  RevenueSummary? report;
  List<Order> todayOrders = const [];
  List<Order> pendingOrders = const [];
  String? error;

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
    final today = dateInput(DateTime.now());
    try {
      final futures = await Future.wait<Object?>([
        if (widget.state.user?.can('reports:read') == true)
          widget.state.api.revenueDay(date: today)
        else
          Future.value(null),
        if (widget.state.user?.can('orders:read') == true)
          widget.state.api.todayOrders(take: 80)
        else
          Future.value(<Order>[]),
        if (widget.state.user?.can('orders:read') == true)
          widget.state.api.pendingOrders(take: 5)
        else
          Future.value(<Order>[]),
      ]);
      setState(() {
        report = futures[0] as RevenueSummary?;
        todayOrders = futures[1] as List<Order>;
        pendingOrders = futures[2] as List<Order>;
      });
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
    if (loading) {
      return const LoadingPane();
    }
    if (error != null) {
      return EmptyState(
        icon: Icons.wifi_off_outlined,
        title: 'Không tải được tổng quan',
        message: error,
      );
    }

    final completed =
        report?.completedCount ??
        todayOrders.where((order) => order.status == 'Hoàn thành').length;
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          if (widget.state.marketData != null) ...[
            MarketDashboard(data: widget.state.marketData!),
            const SizedBox(height: 24),
          ],
          Text(
            'Hôm nay',
            style: Theme.of(
              context,
            ).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: 12),
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
                    value: money(report?.revenue ?? 0),
                    color: Colors.green,
                  ),
                  MetricTile(
                    icon: Icons.pending_actions_outlined,
                    label: 'Đơn chờ duyệt',
                    value: '${pendingOrders.length}',
                    color: Colors.orange,
                  ),
                  MetricTile(
                    icon: Icons.receipt_long_outlined,
                    label: 'Tổng đơn hôm nay',
                    value: '${todayOrders.length}',
                  ),
                  MetricTile(
                    icon: Icons.verified_outlined,
                    label: 'Đã hoàn thành',
                    value: '$completed',
                    color: Colors.blue,
                  ),
                ],
              );
            },
          ),
          const SectionTitle(title: 'Đơn chờ xử lý'),
          if (pendingOrders.isEmpty)
            const EmptyState(
              icon: Icons.done_all,
              title: 'Không có đơn chờ duyệt',
            )
          else
            ...pendingOrders.map(
              (order) => _PendingOrderTile(
                order: order,
                onChanged: _load,
                state: widget.state,
              ),
            ),
          if (report != null && report!.byStatus.isNotEmpty) ...[
            const SectionTitle(title: 'Cơ cấu trạng thái'),
            ...report!.byStatus.map((item) {
              final max = report!.byStatus
                  .map((status) => status.amount)
                  .fold<double>(0, (max, value) => value > max ? value : max);
              return RevenueBar(
                label: item.status,
                value: item.amount,
                maxValue: max,
                trailing: '${item.orderCount} đơn',
              );
            }),
          ],
        ],
      ),
    );
  }
}

class _PendingOrderTile extends StatelessWidget {
  const _PendingOrderTile({
    required this.order,
    required this.onChanged,
    required this.state,
  });

  final Order order;
  final Future<void> Function() onChanged;
  final AppState state;

  @override
  Widget build(BuildContext context) {
    final canManage = state.user?.can('orders:manage') == true;
    return Card(
      child: ListTile(
        title: Text(
          '#${order.orderNumber.isEmpty ? order.id : order.orderNumber}',
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        subtitle: Text(
          '${order.customerName ?? 'Khách lẻ'} • ${order.branchName ?? '--'} • ${displayDateTime(order.orderDate)}',
        ),
        trailing: canManage
            ? Wrap(
                spacing: 4,
                children: [
                  IconButton.filledTonal(
                    tooltip: 'Duyệt',
                    onPressed: () => _decide(context, 'approve'),
                    icon: const Icon(Icons.check),
                  ),
                  IconButton.outlined(
                    tooltip: 'Hủy',
                    onPressed: () => _decide(context, 'cancel'),
                    icon: const Icon(Icons.close),
                  ),
                ],
              )
            : StatusChip(order.status),
        onTap: () => _showOrderDetail(context, order),
      ),
    );
  }

  Future<void> _decide(BuildContext context, String action) async {
    try {
      await state.api.decideOrder(order.id, action);
      if (context.mounted) {
        showSnack(
          context,
          action == 'approve' ? 'Đã duyệt đơn hàng' : 'Đã hủy đơn hàng',
        );
      }
      await onChanged();
    } catch (exception) {
      if (context.mounted) {
        showSnack(context, exception.toString());
      }
    }
  }

  void _showOrderDetail(BuildContext context, Order order) {
    showModalBottomSheet<void>(
      context: context,
      showDragHandle: true,
      builder: (context) => ListView(
        padding: const EdgeInsets.all(16),
        shrinkWrap: true,
        children: [
          Text(
            '#${order.orderNumber}',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 8),
          Text(
            '${order.customerName ?? 'Khách lẻ'} • ${money(order.totalAmount)}',
          ),
          const Divider(),
          ...order.details.map(
            (detail) => ListTile(
              contentPadding: EdgeInsets.zero,
              title: Text(detail.productName ?? 'SP #${detail.productId}'),
              trailing: Text('${detail.quantity} x ${money(detail.unitPrice)}'),
            ),
          ),
        ],
      ),
    );
  }
}
