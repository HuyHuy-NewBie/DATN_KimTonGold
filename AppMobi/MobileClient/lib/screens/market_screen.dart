import 'package:flutter/material.dart';

import '../app_state.dart';
import '../widgets.dart';

class MarketScreen extends StatefulWidget {
  const MarketScreen({super.key, required this.state});

  final AppState state;

  @override
  State<MarketScreen> createState() => _MarketScreenState();
}

class _MarketScreenState extends State<MarketScreen> {
  @override
  Widget build(BuildContext context) {
    final marketData = widget.state.marketData;

    return RefreshIndicator(
      onRefresh: () => widget.state.refresh(),
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          if (marketData == null)
            const EmptyState(
              icon: Icons.cloud_off_outlined,
              title: 'Chưa có dữ liệu thị trường',
              message: 'Vui lòng nhấn làm mới để cập nhật.',
            )
          else ...[
            MarketDashboard(data: marketData),
            const SizedBox(height: 24),
            Text(
              'Ghi chú',
              style: Theme.of(context).textTheme.titleSmall,
            ),
            const SizedBox(height: 8),
            Text(
              '• Dữ liệu được cập nhật tự động từ hệ thống Web Kim Ton.\n'
              '• Tỷ giá này mang tính chất tham khảo cho giao dịch tại quầy.\n'
              '• Biến động 24H so sánh với mức giá chốt phiên ngày hôm trước.',
              style: TextStyle(
                color: Theme.of(context).colorScheme.onSurfaceVariant,
                fontSize: 13,
                height: 1.5,
              ),
            ),
          ],
        ],
      ),
    );
  }
}
