import 'package:flutter/material.dart';

import '../app_state.dart';
import '../formatters.dart';
import '../models.dart';
import '../widgets.dart';

class OrdersScreen extends StatefulWidget {
  const OrdersScreen({super.key, required this.state});

  final AppState state;

  @override
  State<OrdersScreen> createState() => _OrdersScreenState();
}

class _OrdersScreenState extends State<OrdersScreen> {
  bool loading = true;
  String mode = 'pending';
  String from = dateInput(DateTime.now());
  String to = dateInput(DateTime.now());
  String? status;
  int? branchId;
  String? error;
  List<Order> orders = const [];

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
      final data = mode == 'pending'
          ? await widget.state.api.pendingOrders(branchId: branchId, take: 200)
          : await widget.state.api.orderHistory(
              from: from,
              to: to,
              status: status,
              branchId: branchId,
              take: 300,
            );
      setState(() => orders = data);
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
                value: 'pending',
                label: Text('Chờ duyệt'),
                icon: Icon(Icons.pending_actions_outlined),
              ),
              ButtonSegment(
                value: 'history',
                label: Text('Lịch sử'),
                icon: Icon(Icons.history),
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
          if (mode == 'history') ...[
            const SizedBox(height: 10),
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
            const SizedBox(height: 10),
            DropdownButtonFormField<String?>(
              initialValue: status,
              decoration: const InputDecoration(labelText: 'Trạng thái'),
              items: [
                const DropdownMenuItem(
                  value: null,
                  child: Text('Tất cả trạng thái'),
                ),
                ...widget.state.metadata.orderStatuses.map(
                  (item) => DropdownMenuItem(value: item, child: Text(item)),
                ),
              ],
              onChanged: (value) {
                setState(() => status = value);
                _load();
              },
            ),
            const SizedBox(height: 10),
            FilledButton.icon(
              onPressed: _load,
              icon: const Icon(Icons.filter_alt_outlined),
              label: const Text('Lọc đơn hàng'),
            ),
          ],
          const SizedBox(height: 14),
          if (loading)
            const SizedBox(height: 360, child: LoadingPane())
          else if (error != null)
            EmptyState(
              icon: Icons.error_outline,
              title: 'Không tải được đơn hàng',
              message: error,
            )
          else if (orders.isEmpty)
            const EmptyState(
              icon: Icons.receipt_long_outlined,
              title: 'Không có đơn hàng',
            )
          else
            ...orders.map(
              (order) => _OrderTile(
                order: order,
                state: widget.state,
                onChanged: _load,
              ),
            ),
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

class _OrderTile extends StatelessWidget {
  const _OrderTile({
    required this.order,
    required this.state,
    required this.onChanged,
  });

  final Order order;
  final AppState state;
  final Future<void> Function() onChanged;

  @override
  Widget build(BuildContext context) {
    final canManage = state.user?.can('orders:manage') == true;
    final canDecide = canManage && order.status == 'Chờ phê duyệt';
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: Text(
                    '#${order.orderNumber.isEmpty ? order.id : order.orderNumber}',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
                StatusChip(order.status),
              ],
            ),
            const SizedBox(height: 4),
            Text(
              '${order.customerName ?? 'Khách lẻ'} • ${order.branchName ?? '--'}',
            ),
            Text(
              displayDateTime(order.orderDate),
              style: TextStyle(
                color: Theme.of(context).colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                Expanded(
                  child: Text(
                    money(order.totalAmount),
                    style: Theme.of(context).textTheme.titleSmall,
                  ),
                ),
                Text('${order.details.length} SP'),
              ],
            ),
            Wrap(
              spacing: 6,
              children: [
                TextButton.icon(
                  onPressed: () => _showDetail(context),
                  icon: const Icon(Icons.visibility_outlined),
                  label: const Text('Chi tiết'),
                ),
                if (canDecide)
                  TextButton.icon(
                    onPressed: () => _decide(context, 'approve'),
                    icon: const Icon(Icons.check),
                    label: const Text('Duyệt'),
                  ),
                if (canDecide)
                  TextButton.icon(
                    onPressed: () => _decide(context, 'cancel'),
                    icon: const Icon(Icons.close),
                    label: const Text('Hủy'),
                  ),
              ],
            ),
          ],
        ),
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

  void _showDetail(BuildContext context) {
    showModalBottomSheet<void>(
      context: context,
      showDragHandle: true,
      isScrollControlled: true,
      builder: (context) =>
          _OrderDetailSheet(order: order, state: state, onChanged: onChanged),
    );
  }
}

class _OrderDetailSheet extends StatefulWidget {
  const _OrderDetailSheet({
    required this.order,
    required this.state,
    required this.onChanged,
  });

  final Order order;
  final AppState state;
  final Future<void> Function() onChanged;

  @override
  State<_OrderDetailSheet> createState() => _OrderDetailSheetState();
}

class _OrderDetailSheetState extends State<_OrderDetailSheet> {
  late String status = widget.order.status;
  bool saving = false;

  @override
  Widget build(BuildContext context) {
    final canManage = widget.state.user?.can('orders:manage') == true;
    return SafeArea(
      top: false,
      child: ListView(
        padding: const EdgeInsets.all(16),
        shrinkWrap: true,
        children: [
          Text(
            '#${widget.order.orderNumber}',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 8),
          _row('Khách hàng', widget.order.customerName ?? 'Khách lẻ'),
          _row('Số điện thoại', widget.order.customerPhone ?? '--'),
          _row('Chi nhánh', widget.order.branchName ?? '--'),
          _row('Nhân viên', widget.order.staffName ?? '--'),
          _row('Ngày đặt', displayDateTime(widget.order.orderDate)),
          _row('Tổng tiền', money(widget.order.totalAmount)),
          const SectionTitle(title: 'Sản phẩm'),
          ...widget.order.details.map(
            (detail) => ListTile(
              contentPadding: EdgeInsets.zero,
              title: Text(detail.productName ?? 'SP #${detail.productId}'),
              subtitle: Text('Số lượng: ${detail.quantity}'),
              trailing: Text(money(detail.unitPrice)),
            ),
          ),
          if (canManage) ...[
            const SectionTitle(title: 'Cập nhật trạng thái'),
            DropdownButtonFormField<String>(
              initialValue: status,
              items: widget.state.metadata.orderStatuses
                  .map(
                    (item) => DropdownMenuItem(value: item, child: Text(item)),
                  )
                  .toList(),
              onChanged: (value) => setState(() => status = value ?? status),
              decoration: const InputDecoration(labelText: 'Trạng thái'),
            ),
            const SizedBox(height: 12),
            FilledButton.icon(
              onPressed: saving ? null : _saveStatus,
              icon: saving
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.save_outlined),
              label: const Text('Lưu trạng thái'),
            ),
          ],
        ],
      ),
    );
  }

  Widget _row(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 5),
      child: Row(
        children: [
          Expanded(
            child: Text(
              label,
              style: TextStyle(
                color: Theme.of(context).colorScheme.onSurfaceVariant,
              ),
            ),
          ),
          const SizedBox(width: 12),
          Flexible(
            child: Text(
              value,
              textAlign: TextAlign.right,
              style: Theme.of(context).textTheme.labelLarge,
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _saveStatus() async {
    setState(() => saving = true);
    try {
      await widget.state.api.updateOrderStatus(widget.order.id, status);
      if (mounted) {
        showSnack(context, 'Đã cập nhật trạng thái');
        Navigator.pop(context);
      }
      await widget.onChanged();
    } catch (exception) {
      if (mounted) {
        showSnack(context, exception.toString());
      }
    } finally {
      if (mounted) {
        setState(() => saving = false);
      }
    }
  }
}
