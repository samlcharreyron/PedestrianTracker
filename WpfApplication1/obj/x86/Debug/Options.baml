   M S B A M L   `   `   `  ����  PedestrianTracker    PedestrianTracker.Options   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Options��5   	   $	��300��6   $	��300��6&    � Icon$ icons/pedtrac.ico��5   	    � 
ResizeMode$ NoResize=�5   	   .��5	      �  � RowDefinitions 5
   
   �� 5      �� 5      �� 5      $
��Auto �6   �� 5      �� 5      �� 5       � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      .+�5   
   �� $
��Bold"�6   $��0��6"   $��0��6/   .��6?   $"Horizontal angle of Kinect to road6c   �� 5   
   -   5        $��KinectAngleText��5      $��20��6o   $��30��6{   $	��Top=�6�   $��0��6�   $��1��6�   ��6)   ��    Settings �� Source# [   �� Path$ Default.Angle�6�   �� 5   
   $
��Bold"�6   $��1��6"   $��0��6/   .��6?   !Tracking data frames per sample6`   �� 5   
   $
��Bold"�6   $��20��6x   $��30��6�   $	��Top=�6�   $��1��6�   $��1��6�   ��6$   �� # [  $! Default.TrajectorySubsample�6�   �� 5   
   $��0��6   $��2��6    $
��Bold"�6-    �� Target 6?   ��  �� ElementName$ ElevationAngle��.��6m   ElevationAngle6}   �� 5   
   $��1��6   $��2��6%   .+�5      �� {�.  ��6   �� $ ElevationAngle��$ Value�6M   �� 5      -   5        $��ElevationAngle��5      	 �� Orientation$	 Vertical=�6+   
 � Minimum$	
 -27Q�6B    � Maximum$ 27Q�6P   $	��100��6]   $��Center=�6j    �� TickFrequency$ 1Q�5       �� TickPlacement$ BottomRight=�6+    �� IsSnapToTickEnabled .  � Value 6G   �� # [  $ Default.VerticalAngle�5   >   5       �� 5!   
   $
��Bold"�6   $��0��6"   $��3��62   .��6?   '%Milliseconds between database updates6f   �� 5"   
   $��1��6   $��3��6"   $��20��6/   $��60��6<   $	��Top=�6G   ��6`   �� # [  $ Default.dbTimerSetting��� 5#   
   $
��Bold"�6   $��0��6"   $��4��62   .��6?   &$Milliseconds between xml file writes6e   �� 5$   
   $��1��6   $��4��6"   $��20��6/   $��60��6<   $	��Top=�6G   ��6`   �� # [  $ Default.xmlTimerSetting��� 5%   
   $
��Bold"�6   $��5��6"   .��6/   Logging Enabled6@   �� 5&   
   -   5         ��Name$ 	LoggingCb��5&      $��5��6&   $��1��63    d� 	IsChecked$
 TrueY�6   $��Center=�6�   $��Center=�6�   5'      5(      