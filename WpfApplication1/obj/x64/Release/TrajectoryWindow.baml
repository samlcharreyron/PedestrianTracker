   M S B A M L   `   `   `  ���� L  HPedestrianTracker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:PedestrianTrackerPedestrianTracker  (    "PedestrianTracker.TrajectoryWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     .myclr-namespace:PedestrianTracker     -     � Title$  Trajectory Data��$	��600��$	��600�� � Icon$ icons/pedtrac.ico�� � 	Resources +   %PedestrianTracker.TrajectoryDbDataSet   trajectoryDbDataSet  trajectoriesViewSource �� Source �� Path �� Source��%C   &	        &	      7     ��  �� $ trajectories�8   .��&   System.Windows.Controls.DataGrid  -   $��trajectoriesDataGrid��   AutoGenerateColumns .     EnableRowVirtualization . $	��548��   RowDetailsVisibilityMode$ VisibleWhenSelected=�$	��569������ # [ 	   Columns	 0  *System.Windows.Controls.DataGridTextColumn  -   skeletonColumn��,  &System.Windows.Controls.DataGridColumn
   Header$
 skeleton��   Width5  /System.Windows.Controls.DataGridLengthConverter$ SizeToHeader 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� $ skeleton�  -   start_timeColumn��$
 
start time��$ 80  �� $ 
start_time� �� StringFormat$ {0:hh:mm:ss.fff}��  -   end_timeColumn��$
 end time��$ 80  �� $ end_time�$ {0:hh:mm:ss.fff}��  -   average_velocityColumn��$
 average velocity��$ SizeToHeader  �� $ average_velocity�  -   average_directionColumn��$
 average direction��$ SizeToHeader  �� $ average_direction�  -   lengthColumn��$
 length��$ SizeToHeader  �� $ length�  -	   
t_idColumn��$

 t id��   
IsReadOnly . $ SizeToHeader  �� $
 t_id�